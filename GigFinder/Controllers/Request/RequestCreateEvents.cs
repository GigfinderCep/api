using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using GigFinder.Attributes;

namespace GigFinder.Controllers.Request
{
    public class RequestCreateEvents
    {
        [Required(ErrorMessage = "Description is required.")]
        [MinLength(1, ErrorMessage = "Description must be at least 1 character.")]
        [MaxLength(255, ErrorMessage = "Description must be maxium 255 character.")]

        public string Description { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Price must be at least 1.")]
        public int Price { get; set; }

        [Required(ErrorMessage = "Genre is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Genre must be at least 1.")]
        public int GenreId { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        public DateTime DateStart { get; set; }

        [Required(ErrorMessage = "End date is required.")]
        [DateEndValidation(nameof(DateStart))] // Custom validation attribute
        public DateTime DateEnd { get; set; }
    }
}