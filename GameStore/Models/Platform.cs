using System.ComponentModel.DataAnnotations;

namespace GameStore.Models
{
    public enum Platform
    {
        [Display(Name = "PC")]
        PC,
        [Display(Name = "Playstation 4")] 
        PS4,
        [Display(Name = "Playstation 5")] 
        PS5,
        [Display(Name = "Xbox One")] 
        XboxOne,
        [Display(Name = "Xbox Series X/S")] 
        XboxSeries, 
        [Display(Name = "Nintendo Switch")] 
        NintendoSwitch
    }
}
