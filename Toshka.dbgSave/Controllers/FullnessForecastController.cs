using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.TimeSeries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Toshka.dbgSave.DataAccess;
using Toshka.dbgSave.JsonModel;
using Toshka.dbgSave.Model;

namespace Toshka.dbgSave.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FullnessForecastController : ControllerBase
    {
        private readonly EfContext _context;
        public FullnessForecastController(EfContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("predict")]
        public JsonResult GetPredict()
        {
            MLContext mlContext = new MLContext();

            ITransformer modelCopy;
            using (var file = System.IO.File.OpenRead("MLModel.zip"))
                modelCopy = mlContext.Model.Load(file, out DataViewSchema schema);

            var forecastEngineCopy = modelCopy.CreateTimeSeriesEngine<
                ModelInput, ModelOutput>(mlContext);

            ModelOutput forecast = forecastEngineCopy.Predict(7);

            ForecastModel predictionModel = new ForecastModel();
            predictionModel.dayBefore = 0;

            foreach (var prediction in forecast.ForecastedRentals)
            {
                predictionModel.dayBefore++;

                if (prediction >= 70)
                {
                    predictionModel.Fullness = prediction;
                    break;
                }
            }

            return new JsonResult(predictionModel);
        }

        [HttpGet]
        [Route("train")]
        public async Task<IActionResult> Train()
        {
            string rootDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../"));
            string modelPath = Path.Combine(rootDir, "MLModel.zip");

            MLContext mlContext = new MLContext();

            string query = "SELECT \"RentalDate\", \"Id\", CAST(\"Weekend\" as REAL) as Weekend, CAST(\"Export\" as REAL) as Export, CAST(\"Fullness\" as REAL) as Fullness FROM \"ModelsInput\"";

            DatabaseSource dbSource = new DatabaseSource(Npgsql.NpgsqlFactory.Instance,
                                "Host=localhost;Port=5433;Database=safecity;Username=postgres;Password=Hyzazavum1!;",
                                query);

            DatabaseLoader loader = mlContext.Data.CreateDatabaseLoader<ModelInput>();

            IDataView dataView = loader.Load(dbSource);

            var forecastingPipeline = mlContext.Forecasting.ForecastBySsa(
            outputColumnName: "ForecastedRentals",
            inputColumnName: "Fullness",
            windowSize: 7,
            seriesLength: 30,//days interval
            trainSize: _context.ModelsInput.Count(),//data count
            horizon: 7,//predict period
            confidenceLevel: 0.95f,//in %
            confidenceLowerBoundColumn: "LowerBoundRentals",
            confidenceUpperBoundColumn: "UpperBoundRentals");

            SsaForecastingTransformer forecaster = forecastingPipeline.Fit(dataView);
            //test
            Evaluate(dataView, forecaster, mlContext);

            var forecastEngine = forecaster.CreateTimeSeriesEngine<ModelInput, ModelOutput>(mlContext);

            forecastEngine.CheckPoint(mlContext, modelPath);

            Forecast(dataView, 7, forecastEngine, mlContext);

            return Ok();
        }

        static void Forecast(IDataView testData, int horizon, TimeSeriesPredictionEngine<ModelInput, ModelOutput> forecaster, MLContext mlContext)
        {
            ModelOutput forecast = forecaster.Predict();

            IEnumerable<string> forecastOutput =
            mlContext.Data.CreateEnumerable<ModelInput>(testData, reuseRowObject: false)
            .Take(horizon)
            .Select((ModelInput rental, int index) =>
            {
                string rentalDate = rental.RentalDate.ToShortDateString();
                float weekend = rental.Weekend;
                float export = rental.Export;
                float actualFullness = rental.Fullness;
                float lowerEstimate = Math.Max(0, forecast.LowerBoundRentals[index]);
                float estimate = forecast.ForecastedRentals[index];
                float upperEstimate = forecast.UpperBoundRentals[index];
                return $"Weekend: {weekend}\n" +
                $"Export: {export}\n" +
                $"Actual Rentals: {actualFullness}\n" +
                $"Lower Estimate: {lowerEstimate}\n" +
                $"Forecast: {estimate}\n" +
                $"Upper Estimate: {upperEstimate}\n";
            });
        }

        static void Evaluate(IDataView testData, ITransformer model, MLContext mlContext)
        {
            IDataView predictions = model.Transform(testData);

            IEnumerable<float> actual =
            mlContext.Data.CreateEnumerable<ModelInput>(testData, true)
                .Select(observed => observed.Fullness);

            IEnumerable<float> forecast =
            mlContext.Data.CreateEnumerable<ModelOutput>(predictions, true)
                .Select(prediction => prediction.ForecastedRentals[0]);

            var metrics = actual.Zip(forecast, (actualValue, forecastValue) => actualValue - forecastValue);

            var MAE = metrics.Average(error => Math.Abs(error)); // Mean Absolute Error 
            var RMSE = Math.Sqrt(metrics.Average(error => Math.Pow(error, 2))); // Root Mean Squared Error 
        }
    }
}
