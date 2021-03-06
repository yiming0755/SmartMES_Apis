﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMES_Apis.Models;

namespace SmartMES_Apis.Controllers.WorkOrder
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAllOrigin")]
    public class WorkOrdersController : ControllerBase
    {
        private readonly dbContext _context;

        public WorkOrdersController(dbContext context)
        {
            _context = context;
        }

        // GET: api/WorkOrders
        [HttpGet]
        public IEnumerable<PWorkOrder> GetPWorkOrder([FromQuery] string mainOrder)
        {
            if (!String.IsNullOrWhiteSpace(mainOrder))
            {
                return _context.PWorkOrder.Where(e => e.MainOrder.Equals(mainOrder));
            }
            return _context.PWorkOrder;
        }

        // GET: api/WorkOrders/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPWorkOrder([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var pWorkOrder = await _context.PWorkOrder.FindAsync(id);

            if (pWorkOrder == null)
            {
                return NotFound();
            }

            return Ok(pWorkOrder);
        }

        // GET: api/WorkOrders/MainOrders?orderNo=xxx
        [HttpGet("MainOrders")]
        public IEnumerable<PWorkOrder> GetMainOrders([FromQuery] string orderNo)
        {
            var orders = _context.PWorkOrder.Where(e => e.OrderNo.Equals(e.MainOrder));
            if (!String.IsNullOrWhiteSpace(orderNo))
            {
                orders = orders.Where(e => e.OrderNo.Equals(orderNo));
            }
            return orders;
        }

        // api/WorkOrders/Validate?orderNo=xxx
        [HttpGet("Validate")]
        public bool Validate([FromQuery] string orderNo)
        {
            if (String.IsNullOrWhiteSpace(orderNo))
            {
                return false;
            }
            return !_context.PWorkOrder.Any(e => e.OrderNo.Equals(orderNo));
        }

        // PUT: api/WorkOrders/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPWorkOrder([FromRoute] int id, [FromBody] PWorkOrder pWorkOrder)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != pWorkOrder.Id)
            {
                return BadRequest();
            }

            _context.Entry(pWorkOrder).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PWorkOrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/WorkOrders
        [HttpPost]
        public async Task<IActionResult> PostPWorkOrder([FromBody] PWorkOrder pWorkOrder)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var wsCode = (from w in _context.BWorkShop
                          join g in _context.BWorkGroup on w.Wsid equals g.Wsid
                          join f in _context.BProcessFlowDetail on g.GroupCode equals f.ProcessFromGroup
                          where f.FlowCode.Equals(pWorkOrder.FlowCode) && f.Idx == 1
                          select w).FirstOrDefault()?.WsCode;
            pWorkOrder.WorkshopCode = wsCode;
            _context.PWorkOrder.Add(pWorkOrder);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPWorkOrder", new { id = pWorkOrder.Id }, pWorkOrder);
        }

        // POST: api/WorkOrders/SubOrders
        [HttpPost("SubOrders")]
        public async Task<IActionResult> PostPWorkOrder([FromBody] List<PWorkOrder> subOrders)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (subOrders.Count == 0)
            {
                return BadRequest("保存的工单列表不能为空");
            }
            var order = subOrders.First();
            if (_context.PWorkOrder.Where(e => e.MainOrder == order.MainOrder && e.MainOrder != e.OrderNo).ToList().Count > 1)
            {
                return BadRequest("该主工单已拆分");
            }
            _context.PWorkOrder.AddRange(subOrders);
            await _context.SaveChangesAsync();

            return Ok(subOrders);
        }

        // DELETE: api/WorkOrders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePWorkOrder([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var pWorkOrder = await _context.PWorkOrder.FindAsync(id);
            if (pWorkOrder == null)
            {
                return NotFound();
            }
            var orders = new List<PWorkOrder>();
            if (pWorkOrder.MainOrder.Equals(pWorkOrder.OrderNo))
            {
                orders = _context.PWorkOrder.Where(e => e.MainOrder.Equals(pWorkOrder.MainOrder)).ToList();
            }
            else
            {
                var orderList = _context.PWorkOrder.Where(e => e.MainOrder.Equals(pWorkOrder.MainOrder)).ToList();
                orders = GetOrders(pWorkOrder.OrderNo, orderList).ToList();
            }
            foreach (var order in orders)
            {
                _context.PWorkOrder.Remove(order);
            }
            await _context.SaveChangesAsync();

            return Ok(pWorkOrder);
        }

        private bool PWorkOrderExists(int id)
        {
            return _context.PWorkOrder.Any(e => e.Id == id);
        }

        // 递归获取工单以及所有下级所属工单
        private IEnumerable<PWorkOrder> GetOrders (string orderNo, IEnumerable<PWorkOrder> orderList)
        {
            return orderList.Where(e => e.OrderNo.Equals(orderNo))
                            .Concat(orderList.Where(e => e.ParentOrder.Equals(orderNo)).SelectMany(e => GetOrders(e.OrderNo, orderList)));
        }
    }
}