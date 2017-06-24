using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actionable.Data {
    public class UserNotificationEntity {        
        [Key, Column(Order = 0)]
        public virtual string UserIdentity { get; set; }
        [Key, Column(Order = 1)]
        public virtual int Id { get; set; }
        public virtual int Code { get; set; }
        public virtual int Status { get; set; }
        
        public virtual string Message { get; set; }
    }


}
