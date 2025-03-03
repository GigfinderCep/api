using GigFinder.Attributes;
using GigFinder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using GigFinder.Controllers.Request;
using GigFinder.Utils;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Net.Http;

namespace GigFinder.Controllers
{
    public static class MessageTypes
    {
        public static string MESSAGE = "message";
        public static string AUDIO = "audio";
    }
    [RoutePrefix("api/chats")]
    [ProtectedUser]
    public class ChatsController: ApiController
    {

        private gigfinderEntities1 db = new gigfinderEntities1();

        private async Task<ChatRoom> GetChatRoom(int user1, int user2)
        {
            if(user1 == user2)
            {
                throw new Exception("you cant open a chat room with yourself");
            }
            return await db.ChatRooms
               .Where(c => (c.user_id1 == user1 && c.user_id2 == user2)
                        || (c.user_id1 == user2 && c.user_id2 == user1))
               .FirstOrDefaultAsync();
        }

        [HttpGet]
        [Route("{otherUser}/getchat")]
        public async Task<IHttpActionResult> GetUserChatRoom(int otherUser)
        {
            IHttpActionResult response;
            try
            {
                if(otherUser < 1)
                {
                    return BadRequest("invalid other user id");
                }

                User user = UserUtils.GetCurrentUser();

                ChatRoom chatRoom = await GetChatRoom(user.id, otherUser);
    
                if (chatRoom == null)
                {
                    int firstUser = user.id < otherUser ? user.id : otherUser;
                    int secondUser = user.id > otherUser ? user.id : otherUser;
                    ChatRoom newChat = new ChatRoom
                    {
                        user_id1 = firstUser,
                        user_id2 = secondUser,
                        
                    };
                    db.ChatRooms.Add(newChat);
                    await db.SaveChangesAsync();
                    response = Ok(newChat.id);
                }
                else
                {
                    response = Ok(chatRoom.id);
                }
            }catch(Exception e)
            {
                response = BadRequest(e.ToString());
            }
                
            return response;
        }

        [HttpPost]
        [Route("{chatId}/send-message")]
        public async Task<IHttpActionResult> SendMessage(int chatId, [FromBody] RequestSendMessage request)
        {
            try
            {
                if(chatId < 1)
                {
                    return BadRequest("invalid chat id");
                }
                db.Configuration.LazyLoadingEnabled = false;

                if (!ModelState.IsValid)
                {
                    // Return BadRequest with validation errors
                    return BadRequest(ModelState);
                }
                User user = UserUtils.GetCurrentUser();
                ChatRoom chatRoom = await db.ChatRooms.FindAsync(chatId);
                if(chatRoom == null)
                {
                    return BadRequest("chat room not found");
                }
                if(chatRoom.user_id1 != user.id && chatRoom.user_id2 != user.id)
                {
                    return BadRequest("you are not member of this chatroom");
                }

                var msg = new Message
                {
                    chat_id = chatId,
                    sender = user.id,
                    content = request.Content,
                    date = request.Date,
                    type = MessageTypes.MESSAGE
                };
                db.Messages.Add(msg);
                await db.SaveChangesAsync();
                return Ok(msg);
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("{chatId}/send-audio")]
        public async Task<IHttpActionResult> UploadAudio(int chatId)
        {
            try
            {
                if (chatId < 1)
                {
                    return BadRequest("invalid chat id");
                }
                db.Configuration.LazyLoadingEnabled = false;

                // Check if the request contains multipart form data
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return BadRequest("Invalid request type. Must be multipart/form-data.");
                }

                User user = UserUtils.GetCurrentUser();
                ChatRoom chatRoom = await db.ChatRooms.FindAsync(chatId);
                if (chatRoom == null)
                {
                    return BadRequest("chat room not found");
                }
                if (chatRoom.user_id1 != user.id && chatRoom.user_id2 != user.id)
                {
                    return BadRequest("you are not member of this chatroom");
                }

                // Define the server path for storing audio files
                string root = HttpContext.Current.Server.MapPath("~/wwwroot/uploads/audio");
                if (!Directory.Exists(root))
                {
                    Directory.CreateDirectory(root);
                }

                // Read multipart data
                var provider = new MultipartFormDataStreamProvider(root);
                await Request.Content.ReadAsMultipartAsync(provider);
                // get the date
                string dateStr = provider.FormData["date"];

                // Validate datea field
                if (string.IsNullOrWhiteSpace(dateStr) || !DateTime.TryParse(dateStr, out DateTime date))
                {
                    return BadRequest("Invalid date format. Please provide a valid DateTime.");
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

                // Allowed audio formats
                string[] allowedExtensions = { ".mp3" };
                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest("Invalid file type. Only MP3, WAV, OGG, AAC, FLAC, and M4A are allowed.");
                }

                // Create a temporary file entry in the database first
                var file = new GigFinder.Models.File
                {
                    mimetype = extension,
                    path = ""  // Temporary, to be updated after moving the file
                };
                db.Files.Add(file);
                await db.SaveChangesAsync();

                // Use the file's ID as the new file name
                string newFileName = $"{file.id}{extension}";
                string newFilePath = Path.Combine(root, newFileName);

                // Check if the file already exists, delete it before moving the new one
                if (System.IO.File.Exists(newFilePath))
                {
                    System.IO.File.Delete(newFilePath);
                }

                // Move the uploaded file to the final location
                System.IO.File.Move(fileData.LocalFileName, newFilePath);

                // Update the file path in the database
                file.path = $"/wwwroot/uploads/audio/{newFileName}";

                var msg = new Message
                {
                    chat_id = chatId,
                    sender = user.id,
                    content = "",
                    file_identifier = file.id,
                    date = date,
                    type = MessageTypes.AUDIO
                };
                db.Messages.Add(msg);
                await db.SaveChangesAsync();


                // Return the file URL
                return Ok(msg);
            }
            catch (DbEntityValidationException ex)
            {
                // Capture validation errors and return them
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => x.ErrorMessage);

                return BadRequest($"Validation failed: {string.Join(", ", errorMessages)}");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("{chatId}/messages")]
        public async Task<IHttpActionResult> GetChatMessages(int chatId)
        {
            try
            {
                if (chatId < 1)
                {
                    return BadRequest("invalid chat id");
                }
                db.Configuration.LazyLoadingEnabled = false;

                List<Message> messages = await db.Messages
                                                .Where(m => m.chat_id == chatId)
                                                .ToListAsync();
                return Ok(messages ?? new List<Message>());
            }catch(Exception _)
            {
                return Ok(new List<Message>());
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetAllChats()
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;

                if (!ModelState.IsValid)
                {
                    // Return BadRequest with validation errors
                    return BadRequest(ModelState);
                }
                User user = UserUtils.GetCurrentUser();
                List<ChatRoom> chatRooms = await db.ChatRooms.
                                Where(c => c.user_id1 == user.id || c.user_id2 == user.id)
                                .ToListAsync();
 

                return Ok(chatRooms ?? new List<ChatRoom>());
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }

        }
    }
}