using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actionable.Data {
    public class TaskTypeDefinition {
        [Key]
        public virtual int Id { get; set; }
        public virtual string DisplayName { get; set; }
        public virtual string FullyQualifiedName { get; set; }
        public virtual IList<FieldDefinition> Fields { get; set; }
        public virtual string UI { get; set; }
        public virtual IList<string> TypeActions { get; set; }
    }

    public class FieldDefinition {
        [Key]
        public virtual int Id { get; set; }
        public virtual string FullyQualifiedName { get; set; }
        public virtual string DisplayName { get; set; }
        public virtual int FieldType { get; set; }
        public virtual string DefaultValue { get; set; }
    }
    public enum FieldType : int {
        String = 1,
        Int = 2,
        Float = 3,
        DateTime = 4
    }

    public class TypeAction {
        [Key]
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual int EligibleState { get; set; }
        public virtual int TargetState { get; set; }
        public virtual IList<FieldDefinition> RequestFields { get; set; }
    }

    public class TypeActionField {
        [Key]
        public virtual int Id { get; set; }
        public virtual FieldDefinition Field { get; set; }
        public virtual int FieldId { get; set; }
    }

    public class TypeState {
        [Key]
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
    }
}
    