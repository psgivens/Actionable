using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actionable.Data {
    internal class ActionableDbInitializer : DropCreateDatabaseIfModelChanges<ActionableDbContext> {
        protected override void Seed(ActionableDbContext context) {

            // Create some fields
            var titleField = new FieldDefinition {
                FullyQualifiedName = "actionable.title",
                DisplayName = "Title",
                FieldType = (int)FieldType.String,
                DefaultValue = ""
            };
            var descriptionField = new FieldDefinition {
                FullyQualifiedName = "actionable.description",
                DisplayName = "Description",
                FieldType = (int)FieldType.String,
                DefaultValue = ""
            };
            var statusField = new FieldDefinition {
                FullyQualifiedName = "actionable.status",
                DisplayName = "Status",
                FieldType = (int)FieldType.Int,
                DefaultValue = "0"
            };
            var createdDateField = new FieldDefinition {
                FullyQualifiedName = "actioanble.createddate",
                DisplayName = "Created Date",
                FieldType = (int)FieldType.DateTime,
                DefaultValue = DateTimeOffset.Now.ToString ()
            };
            var fields = new List<FieldDefinition>() {
                titleField, descriptionField, statusField, createdDateField
            };
            context.FieldDefinitions.AddRange(fields);

            // Create a type
            var taskType = context.TaskTypeDefinitions.Create();
            taskType.DisplayName = "Action Item";
            taskType.FullyQualifiedName = "actionable.actionitem";
            taskType.Fields = fields;            
            context.TaskTypeDefinitions.Add(taskType);

            //// Create an instance
            //context.TaskInstances.Add(new TaskTypeInstance {
            //    Id = Guid.NewGuid(),
            //    TaskTypeDefinition = taskType,
            //    UserIdentity = "dc85790d-2678-407b-800a-5690c0004497",
            //    StringFields = new List<StringFieldInstance> {
            //        new StringFieldInstance {
            //            FieldDefinition = titleField,
            //            Value = "Do something important with your life"
            //        },
            //        new StringFieldInstance {
            //            FieldDefinition = descriptionField,
            //            Value = "Life is short, spend your time wisely"
            //        }
            //    },
            //    IntFields = new List<IntFieldInstance> {
            //        new IntFieldInstance {
            //            FieldDefinition = statusField,
            //            Value = 0
            //        }
            //    },
            //    DateFields = new List<DateTimeFieldInstance> {
            //        new DateTimeFieldInstance {
            //            FieldDefinition = createdDateField,
            //            Value = DateTimeOffset.Now,
            //        }
            //    }
            //});

            base.Seed(context);
        }
    }
}
