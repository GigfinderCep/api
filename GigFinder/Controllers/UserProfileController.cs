using GigFinder.Models;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using GigFinder.Attributes;
using GigFinder.Utils;
using System.Data.Entity.Validation;

namespace GigFinder.Controllers
{
    [RoutePrefix("api/user")]
    public class UserProfileController : ApiController
    {
        private gigfinderEntities1 db = new gigfinderEntities1();

        [HttpGet]
        [Route("{userId}")]
        public async Task<IHttpActionResult> GetUserProfile(int userId)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;

                var user = await db.Users
                                .Include(u => u.Musician)
                                .Include(u => u.Attachments)
                                .Include(u => u.Musician.Events)
                                .Include(u => u.Local.Events)
                                .Include(u => u.Ratings)
                                .Include(u => u.Local)
                                .Include(u => u.Genres)
                                .Where(u => u.id == userId)
                                .Select(u => new
                                {
                                    u.id,
                                    u.name,
                                    u.description,
                                    u.type,
                                    u.profile_image_identifier,
                                    genres = u.Genres.Select(g => new
                                    {
                                        g.id,
                                        g.name
                                    }),
                                    ratings = u.Ratings.Select(r => new
                                    {
                                        r.User.name,
                                        r.User.profile_image_identifier,
                                        r.content,
                                        r.User.id
                                    }),
                                    attachments = u.Attachments.Select(a => new
                                    {
                                        a.description,
                                        a.id,
                                        a.File.path,
                                        a.File.mimetype
                                    }),
                                    events = u.Local.Events.Concat(u.Musician.Events)
                                                .Select(e => new
                                                {
                                                    e.date_start,
                                                    e.description,
                                                    e.id
                                                })
                                })
                                .FirstOrDefaultAsync();
                if (user == null)
                {
                    return BadRequest("user not found");
                }

                return Ok(user);
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }


        [HttpPost]
        [Route("upload-attachment")]
        [ProtectedUser]
        public async Task<IHttpActionResult> UploadImage()
        {
            try
            {
                // Check if the request contains multipart form data
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return BadRequest("Invalid request type. Must be multipart/form-data.");
                }

                User user = UserUtils.GetCurrentUser();

                // Define the server path for storing images
                string root = HttpContext.Current.Server.MapPath("~/wwwroot/uploads");
                if (!Directory.Exists(root))
                {
                    Directory.CreateDirectory(root);
                }

                // Read multipart data
                var provider = new MultipartFormDataStreamProvider(root);
                await Request.Content.ReadAsMultipartAsync(provider);

                // Extract the string data (e.g., description)
                string description = provider.FormData["description"];
                // Validate description length
                if (string.IsNullOrWhiteSpace(description) || description.Length < 1 || description.Length > 255)
                {
                    return BadRequest("Description must be between 1 and 255 characters.");
                }


                // Process uploaded file
                MultipartFileData fileData = provider.FileData.FirstOrDefault();
                if (fileData == null)
                {
                    return BadRequest("No file uploaded.");
                }

                // Get file extension
                string fileName = Path.GetFileName(fileData.Headers.ContentDisposition.FileName.Trim('"'));
                string extension = Path.GetExtension(fileName).ToLower();
                if (!new[] { ".jpg", ".png", ".jpeg", ".gif" }.Contains(extension))
                {
                    return BadRequest("Invalid file type. Only JPG, PNG, and GIF are allowed.");
                }

                // Create a temporary file entry in the database first
                var file = new GigFinder.Models.File
                {
                    mimetype = extension,
                    path = "" 
                };
                db.Files.Add(file);
                await db.SaveChangesAsync();

                // Use the file's ID as the new file name
                string newFileName = $"{file.id}{extension}";
                string newFilePath = Path.Combine(root, newFileName);
                if (System.IO.File.Exists(newFilePath))
                {
                    // If the file exists, delete it first
                    System.IO.File.Delete(newFilePath);
                }
                // Move file to the final location
                System.IO.File.Move(fileData.LocalFileName, newFilePath);

                // Update the file path in the database
                file.path = $"/wwwroot/uploads/{newFileName}";
                await db.SaveChangesAsync();

                // Create attachment and associate it with the file
                Attachment attachment = new Attachment
                {
                    user_id = user.id,
                    description = description,
                    file_identifier = file.id
                };
                db.Attachments.Add(attachment);
                await db.SaveChangesAsync();

                // Return the file URL
                return Ok(file.path);
            }
            catch (DbEntityValidationException ex)
            {
                // Capture validation errors and log them
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => x.ErrorMessage);

                // Log the errors
                string errorMessage = string.Join(", ", errorMessages);
                return BadRequest($"Validation failed: {errorMessage}");
            }

            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


    }
}