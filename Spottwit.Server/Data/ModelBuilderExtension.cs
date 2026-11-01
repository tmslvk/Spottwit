using Microsoft.EntityFrameworkCore;
using Spottwit.Server.Model;
using Spottwit.Server.Models.Entities;
using System.Reflection;

namespace Spottwit.Server.Data
{
    public static class ModelBuilderExtensions
    {
        public static ModelBuilder RegisterAllEntities(this ModelBuilder modelBuilder, Assembly assembly)
        {
            var entityTypes = assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(BaseEntity)));

            foreach (var type in entityTypes)
                modelBuilder.Entity(type);

            return modelBuilder;
        }
    }
}