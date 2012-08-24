namespace SkyNet.Model
{
    public class Video : File
    {
        /// <summary>
        /// The height, in pixels, of the photo.
        /// </summary>
        public long Height { get; set; }

        /// <summary>
        /// The width, in pixels, of the photo.
        /// </summary>
        public long Width { get; set; }
        
        /// <summary>
        /// A URL of a picture that represents the video.
        /// </summary>
        public string Picture { get; set; }

        /// <summary>
        /// The duration, in milliseconds, of the video run time.
        /// </summary>
        public long Duration { get; set; }

        /// <summary>
        /// The bit rate, in bits per second, of the video.
        /// </summary>
        public long BitRate { get; set; }
    }
}