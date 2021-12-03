using System.Collections.Generic;
using System.Linq;
using Toshka.dbgSave.DataAccess;
using Toshka.dbgSave.Model;
using Toshka.dbgSave.Services.Abstraction;

namespace Toshka.dbgSave.Services
{
    public class CameraService : ICameraService
    {
        private readonly EfContext _context;

        public CameraService(EfContext context)
        {
            _context = context;
        }

        public IEnumerable<Camera> GetAllCameras()
        {
            return _context.Cameras.ToList();
        }

        public void Update(Camera camera)
        {
            _context.Cameras.Update(camera);

            _context.SaveChanges();
        }

        public Camera GetById(long id)
        {
            return _context.Cameras.FirstOrDefault(x => x.Id == id);
        }
    }
}