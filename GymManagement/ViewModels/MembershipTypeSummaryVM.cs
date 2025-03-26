using GymManagement.Models;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.ViewModels
{
    public class MembershipTypeSummaryVM
    {

        [Display(Name = "Membership Type")]
        public string? Membership_Type { get; set; }

        [Display(Name = "Number Of Clients")] 
        public int Number_Of_Clients { get; set; }

        [Display(Name = "Average Fee")]
        [DataType(DataType.Currency)]
        public double Average_Fee { get; set; }

        [Display(Name = "Highest Fee")]
        [DataType(DataType.Currency)]
        public double Highest_Fee { get; set; }

        [Display(Name = "Lowest Fee")]
        [DataType(DataType.Currency)]
        public double Lowest_Fee { get; set; }

        [Display(Name = "Total Fees")]
        [DataType(DataType.Currency)]
        public double Total_Fees { get; set; }

    }
}
