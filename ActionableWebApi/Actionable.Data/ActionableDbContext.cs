using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actionable.Data
{    
    public class ActionableDbContext : DbContext {
        static ActionableDbContext() {
            Database.SetInitializer<ActionableDbContext>(new ActionableDbInitializer());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            modelBuilder.Entity<ActionItemEnvelopeEntity>().ToTable("ActionItemEvents");
            modelBuilder.Entity<UserNotificationEnvelope>().ToTable("UserNotificationEvents");
            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<ActionItemEnvelopeEntity> ActionItemEvents { get; set; }
        public virtual DbSet<UserNotificationEnvelope> UserNotificationEvents { get; set; }
        public virtual DbSet<UserNotificationEntity> UserNotifications { get; set; }
        public virtual DbSet<TaskTypeDefinition> TaskTypeDefinitions { get; set; }
        public virtual DbSet<FieldDefinition> FieldDefinitions { get; set; }
        public virtual DbSet<TaskTypeInstance> TaskInstances { get; set; }
    }
}
