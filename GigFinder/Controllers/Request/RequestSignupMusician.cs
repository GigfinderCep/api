﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GigFinder.Controllers.Request
{
    public class RequestSignupMusician
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

        [Required(ErrorMessage = "Size is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Size must be at least 1 person.")]
        public int Size { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Price must be at least 1 person.")]
        public int Price { get; set; }

        [Required(ErrorMessage = "Lang is required.")]
        public int LangId { get; set; }

        [MinLength(0, ErrorMessage = "Genres array must have at least 0 items.")]
        public int[] Genres { get; set; } = Array.Empty<int>(); // Default to empty array
    }
}