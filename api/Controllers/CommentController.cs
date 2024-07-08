using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Comment;
using api.Dtos.Stock;
using api.Interfaces;
using api.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/comment")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentRepository _commentRepo;
        private readonly IStockRepository _stockRepo;

        public CommentController(ICommentRepository commentRepository, IStockRepository stockRepository)
        {
            _commentRepo = commentRepository;
            _stockRepo = stockRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var comments = await _commentRepo.GetAllAsync();
            var commentDto = comments.Select(s => s.ToCommentDto());

            return Ok(commentDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var comment = await _commentRepo.GetByIdAsync(id);

            if (comment == null)
            {
                return NotFound();
            }

            return Ok(comment.ToCommentDto());
        }

        [HttpPost("{stockId}")]
        public async Task<IActionResult> Create([FromRoute] int stockId, [FromBody] CreateCommentDto createCommentDto)
        {
            if (!await _stockRepo.StockExist(stockId))
            {
                return BadRequest("Stock does not exist!");
            }

            var commentModel = createCommentDto.ToCommentFromCreateDto(stockId);
            await _commentRepo.CreateAsync(commentModel);

            return CreatedAtAction(nameof(GetById), new {id = commentModel.Id}, commentModel.ToCommentDto());
        }

        [HttpPut("{commentId}")]
        public async Task<IActionResult> Update([FromRoute] int commentId, [FromBody] UpdateCommentRequestDto updateCommentRequestDto)
        {
            if (updateCommentRequestDto.StockId.HasValue)
            {
                var stockId = updateCommentRequestDto.StockId;
                if (!await _stockRepo.StockExist(stockId.Value))
                {
                    return BadRequest("StockId does not exist!");    
                }
            }
            else
            {
                return BadRequest("StockId does not exist!");
            }

            var commentModel = await _commentRepo.UpdateAsync(commentId, updateCommentRequestDto);

            if (commentModel == null)
            {
                return NotFound();
            }

            return Ok(commentModel.ToCommentDto());
        }

        [HttpDelete("{commentId}")]
        public async Task<IActionResult> Delete([FromRoute] int commentId){
            var commentModel = await _commentRepo.DeleteAsync(commentId);

            if (commentModel == null)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}