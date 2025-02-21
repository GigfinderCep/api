using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GigFinder.Controllers.Request
{
    public class RequestSignupLocal
    {
        [Required(ErrorMessage = "Name is required.")]
        [MinLength(1, ErrorMessage = "Name must be at least 1 character.")]
        [MaxLength(100, ErrorMessage = "Name must be maxium 100 character.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [MinLength(1, ErrorMessage = "Description must be at least 1 character.")]
        [MaxLength(500, ErrorMessage = "Description must be maxium 500 character.")]

        public string Description { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [MaxLength(100, ErrorMessage = "Email must be maxium 100 character.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(1, ErrorMessage = "Password must be at least 1 character.")]
        [MaxLength(100, ErrorMessage = "Name must be maxium 100 character.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Capacity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1 person.")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "X_coordination is required.")]
        [Range(-180, 180, ErrorMessage = "X_coordination must be between -180 and 180.")]
        public float X_coordination { get; set; }

        [Required(ErrorMessage = "Y_coordination is required.")]
        [Range(-90, 90, ErrorMessage = "Y_coordination must be between -90 and 90.")]
        public float Y_coordination { get; set; }
    }
}