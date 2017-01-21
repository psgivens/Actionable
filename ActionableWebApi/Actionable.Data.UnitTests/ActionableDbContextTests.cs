using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Actionable.Data.UnitTests {
    
    public class ActionableDbContextTests {
        [Fact]
        public void RunSimpleTest() {
            using(var context = new ActionableDbContext()) {
                var taskTypes = context.TaskTypeDefinitions.ToList();

                var items = context.ActionItems;
                Assert.True(items.ToList().Count > 0);
            }
        }

        [Fact]
        public void PopulateSampleData () {
            using (var context = new ActionableDbContext()) {
                var type = context
                    .TaskTypeDefinitions
                    .First(t => t.FullyQualifiedName == "actionable.actionitem");

                //Assert.NotNull(type);
                //context.TaskInstances.Add(new TaskTypeInstance {
                //    TypeDefinition = type,
                //    StringFields 
                //})
                //context.ActionItems.Add(new ActionItemEntity {
                //    Title = "Do something important",
                //    Description = "Do something really important",
                //    Status = 0,
                //    UserIdentity = "dc85790d-2678-407b-800a-5690c0004497"
                //});
                //context.SaveChanges();
                var item = context.ActionItems.First();
                Assert.True(item != null);
            }
        }
    }
}
