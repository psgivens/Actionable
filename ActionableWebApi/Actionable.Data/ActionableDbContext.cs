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
            modelBuilder.Entity<ActionItemEntity>().ToTable("ActionItems");
            modelBuilder.Entity<ActionItemEnvelopeEntity>().ToTable("ActionItemEvents");
            modelBuilder.Entity<NotificationEnvelope>().ToTable("NotificationEvents");
            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<ActionItemEntity> ActionItems { get; set; }
        public virtual DbSet<ActionItemEnvelopeEntity> ActionItemEvents { get; set; }
        public virtual DbSet<NotificationEnvelope> NotificationEvents { get; set; }
        public virtual DbSet<TaskTypeDefinition> TaskTypeDefinitions { get; set; }
        public virtual DbSet<FieldDefinition> FieldDefinitions { get; set; }
        public virtual DbSet<TaskTypeInstance> TaskInstances { get; set; }
    }
}
