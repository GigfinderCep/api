using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GigFinder.Controllers.Request
{
    public class RequestCreateAplication
    {
        [Required(ErrorMessage = "Description is required.")]
        [MinLength(1, ErrorMessage = "Description must be at least 1 character.")]
        [MaxLength(255, ErrorMessage = "Description must be maxium 255 character.")]

        public string Description { get; set; }
    }
}