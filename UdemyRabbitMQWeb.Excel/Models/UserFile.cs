﻿using System.ComponentModel.DataAnnotations.Schema;
using UdemyRabbitMQWeb.Excel.Enums;

namespace UdemyRabbitMQWeb.Excel.Models
{

  
    public class UserFile
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FileName { get; set; }

        public string FilePath { get; set; }
        public DateTime? CreatedDate { get; set; }
        public FileStatus FileStatus { get; set; }

        [NotMapped]
        public string GetCreatedDate => CreatedDate.HasValue ? CreatedDate.Value.ToShortDateString() : "-";

    }
}
