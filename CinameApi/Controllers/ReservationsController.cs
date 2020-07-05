using CinameApi.Data;
using CinameApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CinameApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly CinemaDbContext _dbContext;

        public ReservationsController(CinemaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult GetReservations(int pageNumber = 1, int pageSize = 3)
        {
            var reservationsList = from reservations in _dbContext.Reservations
                                   join customers in _dbContext.Users on reservations.UserId equals customers.Id
                                   join movies in _dbContext.Movies on reservations.MovieId equals movies.Id
                                   select new
                                   {
                                       Id = reservations.Id,
                                       ReservationTime = reservations.ReservationTime,
                                       CustomerName = customers.Name,
                                       MovieName = movies.Name
                                   };

            return Ok(reservationsList.Skip((pageNumber - 1) * pageSize).Take(pageSize));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public IActionResult GetReservationDetail(int id)
        {
            var reservation = (from reservations in _dbContext.Reservations
                               join customers in _dbContext.Users on reservations.UserId equals customers.Id
                               join movies in _dbContext.Movies on reservations.MovieId equals movies.Id
                               where reservations.Id == id
                               select new
                               {
                                   Id = reservations.Id,
                                   ReservationTime = reservations.ReservationTime,
                                   CustomerName = customers.Name,
                                   MovieName = movies.Name,
                                   Email = customers.Email,
                                   Quantity = reservations.Quantity,
                                   Price = reservations.Price,
                                   Phone = reservations.Phone,
                                   PlayingDate = movies.PlayingDate,
                                   PlayingTime = movies.PlayingTime,
                               }).FirstOrDefault();

            return Ok(reservation);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Post([FromBody] Reservation reservation)
        {
            reservation.ReservationTime = DateTime.Now;
            _dbContext.Reservations.Add(reservation);
            _dbContext.SaveChanges();

            return StatusCode(StatusCodes.Status201Created);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var reservation = _dbContext.Reservations.Find(id);

            if (reservation == null)
            {
                return NotFound("Reservation has not been found!");
            }
            else
            {
                _dbContext.Reservations.Remove(reservation);
                _dbContext.SaveChanges();

                return Ok("Reservation deleted!");
            }
        }
    }
}