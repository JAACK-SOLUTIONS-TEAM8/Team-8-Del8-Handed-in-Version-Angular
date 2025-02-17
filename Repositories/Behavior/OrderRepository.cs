﻿using Louman.AppDbContexts;
using Louman.Models.DTOs.Order;
using Louman.Models.DTOs.Product;
using Louman.Models.Entities;
using Louman.Repositories.Abstraction;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Louman.Repositories
{
    public class OrderRepository : IOrderRepository
    {


        private readonly AppDbContext _dbContext;

        public OrderRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<DeliveryTypeDto> AddDeliveryType(DeliveryTypeDto deliveryType)
        {
            if (deliveryType.DeliveryTypeId == 0)
            {
                var newDeliveryType = new DeliveryTypeEntity
                {
                    Description = deliveryType.Description,
                    isDeleted = false
                };
                _dbContext.DeliveryTypes.Add(newDeliveryType);
                await _dbContext.SaveChangesAsync();


                return await Task.FromResult(new DeliveryTypeDto
                {
                    DeliveryTypeId = newDeliveryType.DeliveryTypeId,
                    Description = deliveryType.Description
                });

            }
            else
            {

                var existingDeliveryType = await (from e in _dbContext.DeliveryTypes where e.DeliveryTypeId == deliveryType.DeliveryTypeId && e.isDeleted == false select e).SingleOrDefaultAsync();
                if (existingDeliveryType != null)
                {
                    existingDeliveryType.Description = deliveryType.Description;
                    _dbContext.Update(existingDeliveryType);
                    await _dbContext.SaveChangesAsync();

                    return await Task.FromResult(new DeliveryTypeDto
                    {
                        Description = existingDeliveryType.Description,
                        DeliveryTypeId = existingDeliveryType.DeliveryTypeId
                    });
                }
            }
            return new DeliveryTypeDto();

        }

        public async Task<bool> DeleteDeliveryType(int deliveryTypeId)
        {
            var deliveryType = _dbContext.DeliveryTypes.Find(deliveryTypeId);
            if (deliveryType != null)
            {
                deliveryType.isDeleted = true;
                _dbContext.DeliveryTypes.Update(deliveryType);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }
        public async Task<List<DeliveryTypeDto>> GetAllDeliveryTypes()
        {
            return await (from d in _dbContext.DeliveryTypes
                          where d.isDeleted == false
                          orderby d.Description
                          select new DeliveryTypeDto
                          {
                              Description = d.Description,
                              DeliveryTypeId = d.DeliveryTypeId
                          }).ToListAsync();

        }
        public async Task<DeliveryTypeDto> GetDeliveryTypeById(int deloiveryTypeId)
        {
            return await (from d in _dbContext.DeliveryTypes
                          where d.isDeleted == false && d.DeliveryTypeId == deloiveryTypeId
                          orderby d.Description
                          select new DeliveryTypeDto
                          {
                              Description = d.Description,
                              DeliveryTypeId = d.DeliveryTypeId
                          }).SingleOrDefaultAsync();

        }


        public async Task<GetOrderDto> AddOrder(OrderDto order)
        {
            var newOrderEntity = new OrderEntity
            {
                ClientUserId = order.ClientUserId,
                DeliveryTypeId = order.DeliveryTypeId,
                PaymentType = order.PaymentType,
                OrderStatus = "Pending",
                PickupDate = (order.PickupDate == "null" || order.PickupDate == null) ? null : DateTime.Parse(order.PickupDate),
                PickupTime = (order.PickupTime == "null" || order.PickupTime == null) ? null : DateTime.Parse(order.PickupTime),
                CreatedDate = DateTime.Now,
                isDeleted = false
            };
            _dbContext.Orders.Add(newOrderEntity);
            await _dbContext.SaveChangesAsync();

            foreach (var product in order.Products)
            {
                var orderLineEntity = new OrderLineEntity
                {
                    OrderId = newOrderEntity.OrderId,
                    Quantity = product.Quantity,
                    ProductId = product.Product.ProductId,
                };
                await _dbContext.OrderLines.AddAsync(orderLineEntity);
                await _dbContext.SaveChangesAsync();

                var stockEntity =await  (from s in _dbContext.Stocks where s.ProductId == product.Product.ProductId && s.isDeleted==false
                                         select s).FirstOrDefaultAsync();
                stockEntity.ProductQuantity = stockEntity.ProductQuantity - product.Quantity;
                _dbContext.Stocks.Update(stockEntity);
                await _dbContext.SaveChangesAsync();

            }

            var billEntity = new OrderBillEntity
            {
                OrderId = newOrderEntity.OrderId,
                Total = order.Total,
                Discount = order.Discount
            };
            await _dbContext.OrderBills.AddAsync(billEntity);
            await _dbContext.SaveChangesAsync();

            var cardDetail = _dbContext.CardDetails.Where(card => card.ClientUserId == order.ClientUserId).SingleOrDefault();


            if (cardDetail == null)
            {
                var cardEntity = new CardDetailEntity
                {
                    HolderName = order.CardDetail.HolderName,
                    CardNumber = order.CardDetail.CardNumber,
                    ClientUserId = order.ClientUserId,
                    isDeleted = false,
                    SecurityNumber = order.CardDetail.SecurityNumber
                };
                await _dbContext.CardDetails.AddAsync(cardEntity);
                await _dbContext.SaveChangesAsync();

            }
            else
            {
                cardDetail.CardNumber = order.CardDetail.CardNumber;
                cardDetail.HolderName = order.CardDetail.HolderName;
                cardDetail.SecurityNumber = order.CardDetail.SecurityNumber;
                _dbContext.CardDetails.Update(cardDetail);
                await _dbContext.SaveChangesAsync();

            }
            var auditEntity = new AuditEntity
            {
                Date = DateTime.Now,
                UserId = order.ClientUserId,
                Operation = $"New Order Created!"
            };

            await _dbContext.Audits.AddAsync(auditEntity);
            await _dbContext.SaveChangesAsync();

            return await Task.FromResult(
                    (from o in _dbContext.Orders
                     join u in _dbContext.Users on o.ClientUserId equals u.UserId
                     join dt in _dbContext.DeliveryTypes on o.DeliveryTypeId equals dt.DeliveryTypeId
                     where o.ClientUserId == order.ClientUserId && o.OrderId == newOrderEntity.OrderId
                     select
new GetOrderDto
{
OrderId = newOrderEntity.OrderId,
BillId = billEntity.BillId,
ClientUserId = order.ClientUserId,
OrderStatus = newOrderEntity.OrderStatus,
Total = order.Total,
Discount = order.Discount,
DeliveryType = dt.Description,
CreatedDate = newOrderEntity.CreatedDate,
PaymentType = o.PaymentType,
PickupDate = o.PickupDate.Value.ToString("F"),
PickupTime = o.PickupTime.Value.ToString("F"),
ClientName = $"{u.Name} {u.Surname}"
}).SingleOrDefault());

        }

        public async Task<List<GetOrderDto>> GetAllClientOrders()
        {
            return
                   await (from o in _dbContext.Orders
                          join dt in _dbContext.DeliveryTypes on o.DeliveryTypeId equals dt.DeliveryTypeId
                          join b in _dbContext.OrderBills on o.OrderId equals b.OrderId
                          join u in _dbContext.Users on o.ClientUserId equals u.UserId
                          select new GetOrderDto
                          {
                              OrderId = o.OrderId,
                              BillId = b.BillId,
                              ClientUserId = o.ClientUserId,
                              OrderStatus = o.OrderStatus,
                              Total = b.Total.Value,
                              Discount = b.Discount.Value,
                              DeliveryType = dt.Description,
                              CreatedDate = o.CreatedDate,
                              PaymentType = o.PaymentType,
                              ClientName = $"{u.Name} {u.Surname}",
                              PickupDate = o.PickupDate.Value.ToString("F"),
                              PickupTime = o.PickupTime.Value.ToString("F")
                          }).ToListAsync();
        }
        public async Task<ClientOrderDto> GetAllClientOrderById(int orderId)
        {
            var products = await (from ol in _dbContext.OrderLines
                                  join o in _dbContext.Orders on ol.OrderId equals o.OrderId
                                  join s in _dbContext.Stocks on ol.ProductId equals s.ProductId
                                  join p in _dbContext.Products on ol.ProductId equals p.ProductId
                                  join ps in _dbContext.ProductSizes on p.ProductSizeId equals ps.ProductSizeId
                                  join pt in _dbContext.ProductTypes on p.ProductTypeId equals pt.ProductTypeId
                                  where ol.OrderId == orderId && p.isDeleted==false && o.isDeleted==false
                                  select
      new GetStockProductDto
      {
          ProductId = p.ProductId,
          Price = p.Price,
          ProductName = p.ProductName,
          ProductQuantity = s.ProductQuantity,
          ProductSizeDescription = ps.ProductSizeDescription,
          ProductSizeId = ps.ProductSizeId,
          ProductTypeId = pt.ProductTypeId,
          ProductTypeName = pt.ProductTypeName,
          StockId = s.StockId,

      }).ToListAsync();

            List<ProductInfo> productInfo = new List<ProductInfo>();

            foreach (var product in products)
            {
                var quantity = await (from ol in _dbContext.OrderLines
                                      join o in _dbContext.Orders on ol.OrderId equals o.OrderId
                                      where ol.ProductId == product.ProductId && ol.OrderId == orderId && o.isDeleted==false
                                      select ol.Quantity).FirstOrDefaultAsync();
                productInfo.Add(new ProductInfo { Product = product, Quantity = quantity.Value });
            }

            return
                   await (from o in _dbContext.Orders
                          join dt in _dbContext.DeliveryTypes on o.DeliveryTypeId equals dt.DeliveryTypeId
                          join b in _dbContext.OrderBills on o.OrderId equals b.OrderId
                          join u in _dbContext.Users on o.ClientUserId equals u.UserId
                          join cd in _dbContext.CardDetails on o.ClientUserId equals cd.ClientUserId
                          join a in _dbContext.Addresses on u.AddressId equals a.AddressId
                          where o.OrderId == orderId &&o.isDeleted==false
                          select new ClientOrderDto
                          {
                              OrderId = o.OrderId,
                              BillId = b.BillId,
                              ClientUserId = o.ClientUserId,
                              IdNumber = u.IdNumber,
                              OrderStatus = o.OrderStatus,
                              Total = b.Total.Value,
                              Discount = b.Discount.Value,
                              DeliveryType = dt.Description,
                              CreatedDate = o.CreatedDate,
                              PaymentType = o.PaymentType,
                              ClientName = $"{u.Name} {u.Surname}",
                              DeliveryTypeId = o.DeliveryTypeId,
                              Products = productInfo,
                              Email = u.Email,
                              CityCode = a.CityCode,
                              CityName = a.CityName,
                              StreetName = a.StreetName,
                              StreetNumber = a.StreetNumber,
                              PickupDate = o.PickupDate.Value.ToString("F"),
                              PickupTime = o.PickupTime.Value.ToString("F"),
                              CardDetail = new CardDetailDto { CardNumber = cd.CardNumber, HolderName = cd.HolderName, SecurityNumber = cd.SecurityNumber }
                          }).FirstOrDefaultAsync();
        }

        public async Task<bool> CompleteOrder(int orderId)
        {
            var orderEntity = await _dbContext.Orders.FindAsync(orderId);
            orderEntity.OrderStatus = "Delivered";
            _dbContext.Orders.Update(orderEntity);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        public async Task<List<ProductQuantityDto>> GetMonthlyReport(string dateInfo)
        {

            var date = DateTime.Parse(dateInfo);

            var stockProducts = from s in _dbContext.Stocks
                                join p in _dbContext.Products on s.ProductId equals p.ProductId
                                join ps in _dbContext.ProductSizes on p.ProductSizeId equals ps.ProductSizeId
                                join pt in _dbContext.ProductTypes on p.ProductTypeId equals pt.ProductTypeId
                                select new GetStockProductDto
                                {
                                    ProductId = p.ProductId,
                                    ProductImage = p.ProductImage,
                                    ProductName = p.ProductName,
                                    ProductSizeId = p.ProductSizeId,
                                    ProductTypeId = p.ProductTypeId,
                                    Price = p.ProductId,
                                    ProductQuantity = s.ProductQuantity,
                                    ProductSizeDescription = ps.ProductSizeDescription,
                                    ProductTypeName = pt.ProductTypeName
                                };
            var orders = await (from o in _dbContext.Orders
                                join ol in _dbContext.OrderLines on o.OrderId equals ol.OrderId
                                where o.CreatedDate.Value.Date.Month == date.Date.Month && o.CreatedDate.Value.Date.Year == date.Date.Year
                                select new
                                {
                                    ProductId = ol.ProductId,
                                    Quantity = ol.Quantity
                                }).ToListAsync();

            List<ProductQuantityDto> productQuantity = new List<ProductQuantityDto>();

            foreach (var product in stockProducts)
            {
                int quantity = 0;
                foreach (var order in orders)
                {
                    if (product.ProductId == order.ProductId)
                    {
                        quantity += order.Quantity.Value;
                    }
                }
                productQuantity.Add(new ProductQuantityDto { ProductId = product.ProductId, SoldQuantity = quantity, ProductName = product.ProductName, ProductSize = product.ProductSizeDescription, ProductType = product.ProductTypeName });
            }

            return await Task.FromResult(productQuantity);
        }

        public async Task<List<GetOrderDto>> GetAllClientOrdersByClientUserId(int clientUserId)
        {
            return await (from o in _dbContext.Orders
                          join dt in _dbContext.DeliveryTypes on o.DeliveryTypeId equals dt.DeliveryTypeId
                          join b in _dbContext.OrderBills on o.OrderId equals b.OrderId
                          join u in _dbContext.Users on o.ClientUserId equals u.UserId
                          where u.isDeleted == false && o.isDeleted == false && o.ClientUserId == clientUserId
                          select new GetOrderDto
                          {
                              OrderId = o.OrderId,
                              BillId = b.BillId,
                              ClientUserId = o.ClientUserId,
                              OrderStatus = o.OrderStatus,
                              Total = b.Total.Value,
                              Discount = b.Discount.Value,
                              DeliveryType = dt.Description,
                              CreatedDate = o.CreatedDate,
                              PaymentType = o.PaymentType,
                              ClientName = $"{u.Name} {u.Surname}",
                              PickupDate = o.PickupDate.Value.ToString("F"),
                              PickupTime = o.PickupTime.Value.ToString("F")
                          }).ToListAsync();

        }
    }
}

