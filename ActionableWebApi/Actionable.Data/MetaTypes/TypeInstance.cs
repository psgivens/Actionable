using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actionable.Data {
    public class TaskTypeInstance {
        [Key]
        public virtual Guid Id { get; set; }
        public virtual int TaskTypeDefinitionId { get; set; }
        public virtual string UserIdentity { get; set; }
        public virtual TaskTypeDefinition TaskTypeDefinition { get; set; }
        public virtual IList<IntFieldInstance> IntFields { get; set; }
        public virtual IList<StringFieldInstance> StringFields { get; set; }
        public virtual IList<DateTimeFieldInstance> DateFields { get; set; }
    }
    public class FieldInstanceBase {
        [Key, Column(Order =0)]
        public virtual Guid TaskTypeInstanceId { get; set; }
        [Key, Column(Order = 1)]
        public virtual int FieldDefinitionId { get; set; }
        public virtual TaskTypeInstance TaskTypeInstance { get; set; }
        public virtual FieldDefinition FieldDefinition { get; set; }
    }
    public class IntFieldInstance : FieldInstanceBase {
        public virtual int Value { get; set; }
    }
    public class StringFieldInstance : FieldInstanceBase {
        public virtual string Value { get; set; }
    }
    public class DateTimeFieldInstance : FieldInstanceBase {
        public virtual DateTimeOffset Value { get; set; }
    }
}
