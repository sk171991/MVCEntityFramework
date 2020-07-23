using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVCYorbit.Models
{
    public class AddMember
    {
        public int ID { get; set; }

        public string ApplicationID { get; set; }

        [Required]
        [StringLength(32, MinimumLength = 1, ErrorMessage = " FirstName length cannot be 32 characters long.")]
        public string FirstName { get; set; }

        [StringLength(32, ErrorMessage = " Middle Name length cannot be 32 characters long.")]
        public string MiddleName { get; set; }

        [StringLength(32, MinimumLength = 1, ErrorMessage = " LastName length cannot be 32 characters long.")]
        [Required]
        public string LastName { get; set; }

        [DisplayFormat(NullDisplayText = "Suffix not specified")]
        [Required]
        public string Suffix { get; set; }

        [Required(ErrorMessage = "Kindly choose Date of Birth")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}s")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Select an Option")]
        public string Gender { get; set; }
    }

    public enum Suffix
    {
        Mr,
        Mrs,
        Miss,
        Master
    }
}