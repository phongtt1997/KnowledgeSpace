﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BackendServer.Data.Entities
{
    [Table("LabelInKnowledgeBases")]
    public class LabelInKnowledgeBase
    {
        public int KnowledgeBaseId { get; set; }

        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string LabelId { get; set; }
    }
}
