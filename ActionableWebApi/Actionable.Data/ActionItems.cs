using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actionable.Data {
    public class ActionItemEntity {
        [Key,
         DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public virtual Guid Id { get; set; }
        public virtual String UserIdentity { get; set; }
        public virtual String Title { get; set; }
        public virtual string Description { get; set; }
        public virtual short Status { get; set; }
    }

}
