using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AsposeTestingApp.Models.ViewModels
{
    public class AnalyzeViewModel
    {
        public string Text { get; set; }
        [Range(0, Int32.MaxValue, ErrorMessage = "The field {0} must be greater than {1}.")]
        public int Top { get; set; }
        [Range(0, Int32.MaxValue, ErrorMessage = "WordsToSplit can't be <=0.")]
        public int WordsToSplit {get;set;}
        public bool GrammarChecking { get; set; }
    }
}
