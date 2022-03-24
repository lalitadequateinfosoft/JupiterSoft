using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JupiterSoft.Models
{
    public class FileSystemModel
    {
        public string FileId { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<FileContentModel> fileContents { get; set; }


    }
    public class FileContentModel
    {
        public string ContentId { get; set; }
        public int ContentType { get; set; }
        public string ContentText { get; set; }
        public string ContentValue { get; set; }
        public int ContentOrder { get; set; }
        public double ContentLeftPosition { get; set; }
        public double ContentRightPosition { get; set; }
    }
}
