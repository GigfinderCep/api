using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GigFinder.Controllers.Request
{
    public class RequestUpsertRating
    {
        [Required(ErrorMessage = "Content is required.")]
        [MinLength(1, ErrorMessage = "Content must be at least 1 character.")]
        [MaxLength(255, ErrorMessage = "Content must be maxium 255 character.")]

        public string Content { get; set; }

        [Required(ErrorMessage = "Stars is required.")]
        [Range(1, 5, ErrorMessage = "Stars must be at least 1.")]
        public int Stars { get; set; }

    }
}