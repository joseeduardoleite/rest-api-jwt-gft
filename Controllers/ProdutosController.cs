using System;
using System.Collections.Generic;
using System.Linq;
using APIRestJWT.Containers;
using APIRestJWT.Data;
using APIRestJWT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIRestJWT.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class ProdutosController : ControllerBase
    {
        private readonly AppDbContext _database;
        private Hateoas.Hateoas _hateoas;

        public ProdutosController(AppDbContext database) {
            _database = database;
            _hateoas = new Hateoas.Hateoas("localhost:5001/api/v1/produtos");
            _hateoas.AddAction("get_info", "get");
            _hateoas.AddAction("delete_product", "delete");
            _hateoas.AddAction("edit_product", "patch");
        }

        [HttpGet]
        public IActionResult ListarProduto() {
            var produtos = _database.Produtos.ToList();
            List<ProdutoContainer> produtosHateoas = new List<ProdutoContainer>();

            foreach (var prod in produtos) {
                ProdutoContainer produtoHateoas = new ProdutoContainer();
                produtoHateoas.produto = prod;
                produtoHateoas.links = _hateoas.GetAction(prod.Id.ToString());

                produtosHateoas.Add(produtoHateoas);
            }

            return Ok(produtosHateoas);
        }

        [HttpGet("{id}")]
        public IActionResult ListarProdutoId(int id) {
            try {
                var produtos = _database.Produtos.First(p => p.Id == id);
                ProdutoContainer produtoHateoas = new ProdutoContainer();
                produtoHateoas.produto = produtos;
                produtoHateoas.links = _hateoas.GetAction(produtos.Id.ToString());

                return Ok(produtoHateoas);
            }
            catch {
                Response.StatusCode = 404;
                return new ObjectResult("");
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeletarProduto(int id)
        {
            try {
                Produto produtos = _database.Produtos.First(p => p.Id == id);
                _database.Produtos.Remove(produtos);
                _database.SaveChanges();
                return Ok();
            }
            catch {
                return new ObjectResult("");
            }
        }

        [HttpPost]
        public IActionResult CriarProduto([FromBody] Produto produtoTemporario)
        {
            if (produtoTemporario.Preco <= 0) {
                Response.StatusCode = 400;
                return new ObjectResult(new {msg = "Preço deve ser maior que 0"});
            }

            if (produtoTemporario.Nome.Length <= 1 || produtoTemporario.Nome == null) {
                Response.StatusCode = 400;
                return new ObjectResult(new {msg = "Nome deve ter mais que 1 caracter"});
            }

            Produto produto = new Produto();
            produto.Nome = produtoTemporario.Nome;
            produto.Preco = produtoTemporario.Preco;

            _database.Produtos.Add(produto);
            _database.SaveChanges();

            Response.StatusCode = 201;
            return new ObjectResult("");
        }

        [HttpPatch]
        public IActionResult EditarProduto([FromBody] Produto produto)
        {
            if (produto.Id > 0) {
                try {
                    var produtos = _database.Produtos.FirstOrDefault(p => p.Id == produto.Id);

                    if (produtos != null) {
                        produtos.Nome = produto.Nome != null ? produto.Nome : produtos.Nome;
                        produtos.Preco = produto.Preco != 0 ? produto.Preco : produtos.Preco;

                        _database.SaveChanges();

                        return Ok();
                    }
                    else {
                        return new ObjectResult("Produto não encontrado");
                    }
                }
                catch {
                    return new ObjectResult("Produto não encontrado");
                }
            }
            else {
                Response.StatusCode = 400;
                return new ObjectResult(new {msg = "Id inválido"});
            }
        }

        [HttpGet("teste")]
        public IActionResult TesteClaims() {
            return Ok(HttpContext.User.Claims.First(claim => claim.Type.ToString().Equals("usuarioid", StringComparison.InvariantCultureIgnoreCase)).Value);
        }
    }
}