using APIRestJWT.Hateoas;
using APIRestJWT.Models;

namespace APIRestJWT.Containers
{
    public class ProdutoContainer
    {
        public Produto produto { get; set; }
        public Link[] links { get; set; }
    }
}