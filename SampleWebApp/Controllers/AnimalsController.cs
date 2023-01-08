using Microsoft.AspNetCore.Mvc;
using SampleWebApp.Models;
using SampleWebApp.Services;
using System.Threading.Tasks;

namespace SampleWebApp
{
    [Route("api/animals")]
    [ApiController]
    public class AnimalsController : ControllerBase
    {

        private readonly IDatabaseService _dbService;
        private readonly IDatabaseService2 _dbService2;

        public AnimalsController(IDatabaseService dbService, IDatabaseService2 dbService2)
        {
            _dbService = dbService;
            _dbService2 = dbService2;
        }

        [HttpGet]
        public IActionResult GetAnimals()
        {
            return Ok(_dbService.GetAnimals());
        }

        [HttpPost("CreateAnimal")]
        public IActionResult CreateAnimal(Animal animal)
        {
            //fake AnimalId - not sure if SQL Server detects potential collisions
            animal.IdAnimal = -1;
            int resId = _dbService.CreateAnimal(animal);

            if (resId == 0)
            {
                return BadRequest("Cannot create new animal");
            }
            else {
                return Ok(resId);
            }

        }



        [HttpGet("{IdAnimal}")]
        public IActionResult GetAnimal(int IdAnimal)
        {
            Animal newAnimal = null;
            newAnimal = (Animal)_dbService.GetAnimal(IdAnimal);

            if (newAnimal == null)
            {
                return BadRequest("Animal with id: " + IdAnimal + " not found.");
            }
            else {
                return Ok(newAnimal);
            }
        }



        [HttpPut("{IdAnimal}/UpdateAnimal")]
        public IActionResult UpdateAnimal(int IdAnimal, Animal animal)
        {
            bool updatedAnimal = false;
            updatedAnimal = (bool) _dbService.UpdateAnimal(IdAnimal, animal);

            if (updatedAnimal == false)
            {
                return BadRequest("Animal with id: " + IdAnimal + " not found.");
            }
            else
            {
                return Ok(updatedAnimal);
            }
        }


        [HttpDelete("{IdAnimal}/DeleteAnimal")]
        public IActionResult DeleteAnimal(int IdAnimal)
        {
            bool deletedAnimal = false;
            deletedAnimal = (bool)_dbService.DeleteAnimal(IdAnimal);

            if (deletedAnimal == false)
            {
                return BadRequest("Animal was not deleted");
            }
            else
            {
                return Ok(deletedAnimal);
            }
        }








        //Other methods

        [HttpGet("proc")]
        public async Task<IActionResult> GetAnimalsByProc()
        {
            return Ok(await _dbService2.GetAnimalsByStoredProcedureAsync());
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAnimals()
        {
            return Ok(await _dbService2.ChangeAnimalAsync());
        }

        [HttpGet("async")]
        public async Task<IActionResult> GetAnimals2()
        {
            return Ok(await _dbService2.GetAnimalsAsync());
        }

    }
}
