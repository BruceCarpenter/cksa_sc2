using System.Data;
/// <summary>
/// Summary description for GetVideos
/// </summary>
/// 
namespace ckLib
{
    public class GetVideos
    {
        static public List<Video> GetVideosForIdeas(int ideaId)
        {
            var videos = new List<Video>();

            try
            {
                using(var conn = DbDriver.OpenConnection())
                using (var command = conn.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT a.VName, a.Description, a.Url from Videos as a INNER JOIN VideoIdea as b on a.id = b.videoid where b.IdeaId = @c0";
                    command.Parameters.AddWithValue("@c0", ideaId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var video = new Video();
                            video.Name = reader.GetString(0);
                            video.Description = reader.GetString(1);
                            if (!reader.IsDBNull(2))
                            {
                                video.Url = reader.ReadString(2);
                                videos.Add(video);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.Handle(new ckExceptionData(ex, "GetVideos::GetVideosForIdeas", ideaId.ToString()));
            }

            return videos;
        }
    }
}