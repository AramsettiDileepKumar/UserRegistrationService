using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;
using UserManagementService.DTO;
using UserManagementService.Entity;
using UserManagementService.Interface;
using UserManagementService.Model;

namespace UserManagementService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserManagementController : ControllerBase
    {
        private readonly IUser _user;
        public UserManagementController(IUser user)
        {
            _user = user;
        }
        [HttpPost("signup/admin")]
        public async Task<IActionResult> AdminRegistration(AdminRegistrationModel user)
        {
            try
            {
                var addedUser = await MapToEntity(user);
                if (addedUser)
                {
                    var response = new ResponseModel<bool>
                    {
                        StatusCode=200,
                        Success = true,
                        Message = "User Registration Successful",
                        Data=addedUser
                    };
                    return Ok(response);
                }
                else
                {
                    return BadRequest("invalid input");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while adding the user: {ex.Message}");
            }
        }
        [HttpPost("signUp/patient")]
        public async Task<IActionResult> PatientRegistration(PatientRegistrationModel user)
        {
            try
            {
                var addedUser = await MapToEntity(user);
                if (addedUser)
                {
                    var response = new ResponseModel<bool>
                    {
                        Success = true,
                        Message = "User Registration Successful",
                        Data = addedUser
                    };
                    return Ok(response);
                }
                else
                {
                    return BadRequest("invalid input");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while adding the user: {ex.Message}");
            }
        }
        [HttpPost("signup/doctor")]
        public async Task<IActionResult> DoctorRegistration(DoctorRegistrationModel user)
        {
            try
            {
                var addedUser = await MapToEntity(user);
                if (addedUser)
                {
                    var response = new ResponseModel<bool>
                    {
                        Success = true,
                        Message = "User Registration Successful",
                        Data = addedUser
                    };
                    return Ok(response);
                }
                else
                {
                    return BadRequest("invalid input");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while adding the user: {ex.Message}");
            }
        }
        [HttpPost("login")]
        public async Task<IActionResult> UserLogin(UserLoginModel userlogin)
        {
            try
            {
                var login = await _user.UserLogin(userlogin);
                if (login!=null)
                {
                    var response = new ResponseModel<string>
                    {
                        StatusCode=200,
                        Success = true,
                        Message = "User Login Successful",
                        Data = login
                    };
                    return Ok(response);
                }
                else
                {
                    var respons = new ResponseModel<string>
                    {
                        StatusCode =400,
                        Success = false,
                        Message = "Invalid User",
                        Data = login
                    };
                    return Ok(respons);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while adding the user: {ex.Message}");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetDetails()
        {
            try
            {
                var result = await _user.GetUsers();
                if (result != null)
                {
                    var response = new ResponseModel<IEnumerable<UserResponse>>
                    {
                        StatusCode = 200,
                        Success = true,
                        Message = "Users  Details Fetched Successfully",
                        Data=result
                    };
                    return Ok(response);
                }
                return BadRequest(new ResponseModel<UserEntity>
                {
                    StatusCode = 400,
                    Success = false,
                    Message = "User Not Found"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        private async Task<bool> MapToEntity<T>(T model)
        { 
            UserEntity user = null;

            if (model is AdminRegistrationModel adminModel)
            {
                user = new UserEntity
                {
                    FirstName = adminModel.FirstName,
                    LastName = adminModel.LastName,
                    Email = adminModel.Email,
                    Password = adminModel.Password,
                    Role = "Admin"
                };
            }
            else if (model is PatientRegistrationModel patientModel)
            {
                user = new UserEntity
                {
                    FirstName = patientModel.FirstName,
                    LastName = patientModel.LastName,
                    Email = patientModel.Email,
                    Password = patientModel.Password,
                    Role = "Patient"
                };
            }
            else if (model is DoctorRegistrationModel DoctorModel)
            {
                user = new UserEntity
                {
                    FirstName = DoctorModel.FirstName,
                    LastName = DoctorModel.LastName,
                    Email = DoctorModel.Email,
                    Password = DoctorModel.Password,
                    Role = "Doctor"
                };
            }
            if (user == null)
            {
                return false;
            }
            var result = await _user.RegisterUser(user);
            return result;
        }
    }
    }
   




