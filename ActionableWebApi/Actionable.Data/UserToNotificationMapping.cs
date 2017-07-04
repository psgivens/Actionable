using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actionable.Data {
    public class UserToNotificationMapping {
        [Key]
        public virtual string UserId { get; set; }
        public virtual Guid UserNotificationStreamId { get; set; }
    }
}
