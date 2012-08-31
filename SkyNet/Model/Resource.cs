namespace SkyNet.Model
{
    public class Resource
    {
        /// <summary>
        ///   The File object's ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///   Info about the user who uploaded the file.
        /// </summary>
        public From From { get; set; }

        /// <summary>
        ///   The name of the file.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///   A description of the file, or null if no description is specified.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///   The ID of the folder the file is currently stored in.
        /// </summary>
        public string Parent_Id { get; set; }

        /// <summary>
        ///   The URL to upload file content hosted in SkyDrive.  This structure is not available if the file is a Microsoft Office OneNote notebook.
        /// </summary>
        public string Upload_Location { get; set; }

        /// <summary>
        ///   A value that indicates whether this file can be embedded. If this file can be embedded, this value is true; otherwise, it is false.
        /// </summary>
        public bool Is_Embeddable { get; set; }

        /// <summary>
        ///   A URL to view the item on SkyDrive.
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        ///   The type of object; in this case, "file". If the file is a Office OneNote notebook, the type structure is set to "notebook".
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        ///   Object that contains permission info.
        /// </summary>
        public SharedWith Shared_With { get; set; }

        /// <summary>
        /// The time, in ISO 8601 format, at which the file was created.
        /// </summary>
        public string Created_Time { get; set; }

        /// <summary>
        /// The time, in ISO 8601 format, at which the file was last updated.
        /// </summary>
        public string Updated_Time { get; set; }
    }
}