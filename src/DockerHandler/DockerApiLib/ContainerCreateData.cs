namespace DockerApiLib
{
    public class ContainerCreateData
        {
            public string Image { get; set; }
            public string Name { get; set; }

            public ContainerCreateData(string image, string name)
            {
                if (image == null || string.IsNullOrEmpty(image))
                    throw new ArgumentNullException("image");
                if (name == null || string.IsNullOrEmpty(name))
                    throw new ArgumentNullException("name");

                Image = image;
                Name = name;
            }
        }
    
}
