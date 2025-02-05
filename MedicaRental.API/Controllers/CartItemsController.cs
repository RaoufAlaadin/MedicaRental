﻿using MedicaRental.BLL.Dtos;
using MedicaRental.BLL.Dtos.Admin;
using MedicaRental.BLL.Managers;
using MedicaRental.DAL.Context;
using MedicaRental.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedicaRental.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CartItemsController : Controller
{
    private readonly ICartItemsManager _cartItemsManager;
    private readonly UserManager<AppUser> _userManager;

    public CartItemsController(ICartItemsManager cartItemsManager, UserManager<AppUser> userManager)
    {
        _cartItemsManager = cartItemsManager;
        _userManager = userManager;
    }

    [HttpGet]
    [Authorize(Policy = ClaimRequirement.ClientPolicy)]
    public async Task<ActionResult<IEnumerable<CartItemDto>>> GetCartItems()
    {
        var userId = _userManager.GetUserId(User);
        IEnumerable<CartItemDto> cartItems = await _cartItemsManager.GetCartItemsAsync(userId);
        return Ok(cartItems);
    }

    [HttpGet]
    [Route("IsInCart/{itemId}")]
    public async Task<ActionResult<bool>> IsInCart(Guid ItemId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return false;

        var claims = await _userManager.GetClaimsAsync(user);
        var userRole = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
        if (userRole is null || userRole.Value != UserRoles.Client.ToString()) return false;

        return await _cartItemsManager.IsInCartAsync(ItemId, user.Id);
    }

    [HttpPost]
    [Authorize(Policy = ClaimRequirement.ClientPolicy)]
    public async Task<ActionResult<StatusDto>> AddToCart(AddToCartRequestDto addToCartRequest)
    {
        var userId = _userManager.GetUserId(User);
        StatusDto addToCartResult = await _cartItemsManager.AddToCartAsync(addToCartRequest, userId);
        return StatusCode((int)addToCartResult.StatusCode, addToCartResult);
    }

    [HttpDelete]
    [Route("{itemId}")]
    [Authorize(Policy = ClaimRequirement.ClientPolicy)]
    public async Task<ActionResult<StatusDto>> RemoveCartItem(Guid itemId)
    {
        var userId = _userManager.GetUserId(User);
        StatusDto removeFromCartResult = await _cartItemsManager.RemoveCartItemAsync(itemId, userId);
        return StatusCode((int)removeFromCartResult.StatusCode, removeFromCartResult);
    }
}
