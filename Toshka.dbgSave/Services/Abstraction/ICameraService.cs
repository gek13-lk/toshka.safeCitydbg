using System.Collections.Generic;
using Toshka.dbgSave.Model;

namespace Toshka.dbgSave.Services.Abstraction
{
    public interface ICameraService
    {
        public void Update(Camera camera);
        public IEnumerable<Camera> GetAllCameras();

        public Camera GetById(long id);
    }
}