using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla.DTO;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;

namespace RoyalVilla_API.Controllers.v2
{
    [Route("api/v{version:apiVersion}/villa-amenities")]
    [ApiController]
    [ApiVersion("2.0")]
    //[Authorize(Roles = "Customer,Admin")]
    public class VillaAmenitiesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public VillaAmenitiesController(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet]
        //[Authorize(Roles ="Admin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaAmenitiesDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<VillaAmenitiesDTO>>>> GetVillaAmenities()
        {
            var villas = await _db.VillaAmenities.ToListAsync();
            var dtoResponseVillaAmenities = _mapper.Map<List<VillaAmenitiesDTO>>(villas);
            var response = ApiResponse<IEnumerable<VillaAmenitiesDTO>>.Ok(dtoResponseVillaAmenities, "Villas retrieved successfully");
            return Ok(response);
        }

        [HttpGet("{id:int}")]
        //[Authorize(Roles ="Customer,Admin")]
        [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<VillaAmenitiesDTO>>> GetVillAmenitiesById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return NotFound(ApiResponse<object>.NotFound("VillaAmenities ID must be greater than 0"));
                }

                var villaAmenities = await _db.VillaAmenities.FirstOrDefaultAsync(x => x.Id == id);
                if (villaAmenities == null)
                {
                    return NotFound($"VillaAmenities with ID {id} was not found");
                }

                return Ok(ApiResponse<VillaAmenitiesDTO>.Ok(_mapper.Map<VillaAmenitiesDTO>(villaAmenities), "Records retrieved successfully"));

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occured while retrieving villa with ID {id}: {ex.Message}");
            }
        }

        [HttpPost]
        //[Authorize]
        [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<VillaAmenitiesDTO>>> CreateVillaAmenities(VillaAmenitiesCreateDTO villaAmenitiesDTO)
        {
            try
            {
                if (villaAmenitiesDTO == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Villa Amenities data is required"));
                }

                var villaExists = await _db.Villa.FirstOrDefaultAsync
                    (u => u.Id == villaAmenitiesDTO.VillaId);

                if (villaExists == null)
                {
                    return Conflict(ApiResponse<object>.Conflict($"Villa with the Id '{villaAmenitiesDTO.VillaId}' already exisits"));
                }

                VillaAmenities villaAmenities = _mapper.Map<VillaAmenities>(villaAmenitiesDTO);
                villaAmenities.CreatedDate = DateTime.Now;
                await _db.VillaAmenities.AddAsync(villaAmenities);
                await _db.SaveChangesAsync();


                var response = ApiResponse<VillaAmenitiesDTO>.CreatedAt(_mapper.Map<VillaAmenitiesDTO>(villaAmenities), "VillaAmenities created successfully");
                return CreatedAtAction(nameof(CreateVillaAmenities), new { id = villaAmenities.Id }, response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An error occured while creating the villa", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPut("{id:int}")]
        //[Authorize]
        [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<VillaAmenitiesDTO>>> UpdateVillaAmenities(int id, VillaAmenitiesUpdateDTO villaAmenitiesUpdateDTO)
        {
            try
            {
                if (villaAmenitiesUpdateDTO == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("VillaAmenities data is required"));
                }

                if (id != villaAmenitiesUpdateDTO.Id)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("VillaAmenities ID in URL does not match with VillaAmenities ID in request body"));
                }

                var villaExists = await _db.Villa.FirstOrDefaultAsync
                    (u => u.Id == villaAmenitiesUpdateDTO.VillaId);

                if (villaExists == null)
                {
                    return Conflict(ApiResponse<object>.Conflict($"Villa with the Id '{villaAmenitiesUpdateDTO.VillaId}' does not exist"));
                }

                var existingVillaAmenities = await _db.VillaAmenities.FirstOrDefaultAsync(u => u.Id == id);

                if (existingVillaAmenities == null)
                {
                    return NotFound($"Villa Amenities with ID {id} was not found");
                }

                _mapper.Map(villaAmenitiesUpdateDTO, existingVillaAmenities);

                existingVillaAmenities.UpdatedDate = DateTime.Now;

                await _db.SaveChangesAsync();

                var response = ApiResponse<VillaAmenitiesDTO>.Ok(_mapper.Map<VillaAmenitiesDTO>(existingVillaAmenities), "Villa Amenities updated successfully");

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An error occured while updating the villa amenities", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpDelete("{id:int}")]
        //[Authorize]
        [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteVillaAmenities(int id)
        {
            try
            {
                var existingVillaAmenities = await _db.VillaAmenities.FirstOrDefaultAsync(u => u.Id == id);

                if (existingVillaAmenities == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Villa Amenities with ID {id} was not found"));
                }

                _db.VillaAmenities.Remove(existingVillaAmenities);
                await _db.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An error occured while deleting the villa amenities", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }
    }
}
