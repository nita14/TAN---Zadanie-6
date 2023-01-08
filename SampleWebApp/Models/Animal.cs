using System.ComponentModel.DataAnnotations;

namespace SampleWebApp.Models
{
    public class Animal
    {


        [Required(ErrorMessage = "Animal's name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Animal's description is required")]
        [MaxLength(200, ErrorMessage = "Max 200 chars are allowed")]
        public string Description { get; set; }

        //[Required(ErrorMessage="Animal's id is required")]
        public int IdAnimal { get; set; }

        [MaxLength(200, ErrorMessage = "Max 200 chars are allowed")]
        public string Category { get; set; }
        [MaxLength(200, ErrorMessage = "Max 200 chars are allowed")]
        public string Area { get; set; }
    }
}