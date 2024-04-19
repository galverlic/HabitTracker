using Postgrest.Attributes;
using Postgrest.Models;

namespace HabitTracker.Models
{
    [Table("users")] // Correctly identifies the PostgreSQL table name
    public class User : BaseModel // Ensure User inherits from BaseModel for Postgrest
    {
        [PrimaryKey("user_id", shouldInsert: true)] // The name here should match the actual primary key column name in your table
        public string UserId { get; set; }

        [Column("name")] // Maps the Name property to the "name" column in the PostgreSQL table
        public string Name { get; set; }

        [Column("email")] // Maps the Email property to the "email" column in the PostgreSQL table
        public string Email { get; set; }  // Should match with the email used in Supabase auth

        [Column("profile_picture_url")] // Maps the ProfilePictureUrl property to the "profile_picture_url" column
        public string ProfilePictureUrl { get; set; }

        [Column("date_of_birth")] // Maps the DateOfBirth property to the "date_of_birth" column
        public DateTime DateOfBirth { get; set; }
    }
}
