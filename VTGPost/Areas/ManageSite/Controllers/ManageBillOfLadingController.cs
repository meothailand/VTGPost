using GemBox.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using VTGPost.Helper;
using VTGPost.Models;
using VTGPost.Models.BaseModel;

namespace VTGPost.Areas.ManageSite.Controllers
{
    public class ManageBillOfLadingController : BaseController
    {
        // GET: /ManageSite/ManageBillOfLading/

        public ViewResult Bills(int page = 1)
        {
           
                if (HttpContext.Session[SiteConfig.TransferMessageSession] != null)
                {
                    ViewBag.SaveMessage = HttpContext.Session[SiteConfig.TransferMessageSession];
                    HttpContext.Session[SiteConfig.TransferMessageSession] = null;
                }

                var searchCriteria = Session[SiteConfig.BillSearchCriteriaSession] == null ?
                                     new BillFilterCriteria(){ SentDateFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1) } :
                                     (BillFilterCriteria)Session[SiteConfig.BillSearchCriteriaSession];

                Session[SiteConfig.BillSearchCriteriaSession] = Session[SiteConfig.BillSearchCriteriaSession] == null ?
                                                                searchCriteria :
                                                                Session[SiteConfig.BillSearchCriteriaSession];

                var customers = new List<Customer>();
                var bills = SearchBill(searchCriteria, out customers);
                var paging = PagingHelper.Paging(bills.Count, 30, page);
                ViewBag.Paging = paging;
                ViewBag.Customers = new SelectList(customers, "Id", "CustomerName", searchCriteria.CustomerId);
                return View(bills.GetRange(paging.StartRecord - 1, paging.EndRecord + 1 - paging.StartRecord));
           
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ViewResult Bills(BillFilterCriteria filterCriteria, string SentDateFrom, string SentDateTo)
        {
           
                filterCriteria.SentDateFrom = SetDateTime(SentDateFrom);
                filterCriteria.SentDateTo = SetDateTime(SentDateTo);
                Session[SiteConfig.BillSearchCriteriaSession] = filterCriteria;
                var customers = new List<Customer>();
                var bills = SearchBill(filterCriteria, out customers);
                var paging = PagingHelper.Paging(bills.Count, 30, 1);
                ViewBag.Paging = paging;
                ViewBag.Customers = new SelectList(customers, "Id", "CustomerName", filterCriteria.CustomerId);
                return View(bills.GetRange(paging.StartRecord - 1, paging.EndRecord + 1 - paging.StartRecord));

        }


        public ViewResult AddBill()
        {
            using(var context = new WebsiteDBEntities())
            {
                ViewBag.Customer = new SelectList(context.Customers.ToList(), "Id", "CustomerName", null);
                return View();
            }
        }

        [HttpPost]
        public ActionResult AddBill(BillOfLading bill, string sentDate, string receivedDate, string retransferTime1, string retransferTime2, string btnContinue)
        {
            try
            {
                ConvertBillDateTime(bill, sentDate, receivedDate, retransferTime1, retransferTime2);
                CheckBillStatus(bill, string.Empty);

                using (var context = new WebsiteDBEntities())
                {
                    ViewBag.Customer = new SelectList(context.Customers.ToList(), "Id", "CustomerName", bill.Customer);
                    bill.BillCode = bill.BillCode.ToUpper();
                    if(bill.Customer.HasValue){ GetSenderInfoByCustomer(bill);}
                    context.BillOfLadings.Add(bill);
                    context.SaveChanges();
                    if (btnContinue == null)
                    {
                        HttpContext.Session[SiteConfig.TransferMessageSession] = string.Format("Vận đơn mã {0} tạo thành công", bill.BillCode);
                        return RedirectToAction("Bills");
                    }
                    var newBill = new BillOfLading
                                               {
                                                   BillCode = "00000000",
                                                   Sender = bill.Sender,
                                                   SenderAddress = bill.SenderAddress,
                                                   SenderContactName = bill.SenderContactName,
                                                   SenderTel = bill.SenderTel,
                                                   SentDate = DateTime.Today,
                                                   LadingStatus = SiteConfig.ProcessStatus,
                                                   Customer = bill.Customer,
                                                   IsReadOnly = false
                                               };
                    ViewBag.SaveMessage = string.Format("Vận đơn mã {0} tạo thành công", bill.BillCode);
                    return View("AddBill", newBill);
                }
            }
            catch (Exception ex)
            {
                ViewBag.SaveMessage = "Error: " + ex.Message;
                return View(bill);
            }
        }

        public ActionResult EditBill(string billCode)
        {
            using (var context = new WebsiteDBEntities())
            {
                var bill = context.BillOfLadings.SingleOrDefault(i => i.BillCode == billCode.ToUpper());
                if (bill == null)
                {
                    Session[SiteConfig.TransferMessageSession] = "Thông tin bạn tìm không tồn tại";
                    return RedirectToAction("Bills", "ManageBill");
                }
                ViewBag.Customer = new SelectList(context.Customers.ToList(), "Id", "CustomerName", bill.Customer);
                return View(bill);
            }
        }

        [HttpPost]
        public ActionResult EditBill(BillOfLading bill, string sentDate, string receivedDate, string retransferTime1, string retransferTime2)
        {
            try
            {
                using (var context = new WebsiteDBEntities())
                {
                    ViewBag.Customer = new SelectList(context.Customers.ToList(), "Id", "CustomerName", bill.Customer);
                    var curentBill = context.BillOfLadings.SingleOrDefault(i => i.BillCode == bill.BillCode.ToUpper());

                    if (curentBill == null)
                    {
                        ViewBag.SaveMessage = "Error: Không tìm thấy thông tin vận đơn yêu cầu.";
                    }
                    
                    ConvertBillDateTime(bill, sentDate, receivedDate, retransferTime1, retransferTime2);
                    CheckBillStatus(bill, curentBill.LadingStatus);
                    if (curentBill.Customer != bill.Customer) GetSenderInfoByCustomer(bill);
                    ObjectHelper.Copy(bill, curentBill);
                    context.Entry(curentBill).State = EntityState.Modified;
                    context.SaveChanges();
                    ViewBag.SaveMessage = "Vận đơn lưu thành công.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.SaveMessage = "Error: " + ex.Message;
            }

            return View(bill);
        }

        public ActionResult CopyBill(string code)
        {
            using(var context = new WebsiteDBEntities())
            {
                var bill = context.BillOfLadings.SingleOrDefault(i => i.BillCode == code);

                if(bill == null)
                {
                    HttpContext.Session[SiteConfig.TransferMessageSession] = "";
                    return RedirectToAction("Bills");
                }

                ViewBag.Customer = new SelectList(context.Customers.ToList(), "Id", "CustomerName", bill.Customer);
                var newBill = new BillOfLading
                                {
                                    BillCode = "00000000",
                                    Sender = bill.Sender,
                                    SenderAddress = bill.SenderAddress,
                                    SenderContactName = bill.SenderContactName,
                                    SenderTel = bill.SenderTel,
                                    SentDate = DateTime.Today,
                                    LadingStatus = SiteConfig.ProcessStatus,
                                    Customer = bill.Customer,
                                    IsReadOnly = false
                                };
                return View("AddBill", newBill);
            }    
        }

        public ActionResult DeleteBill(string billCode)
        {
            using(var context = new WebsiteDBEntities())
            {
                var billToDelete = context.BillOfLadings.SingleOrDefault(i => i.BillCode == billCode.ToUpper());
                if (billToDelete == null || billToDelete.IsReadOnly)
                {
                    HttpContext.Session[SiteConfig.TransferMessageSession] = string.Format("Vận đơn mã {0} không tồn tại hoặc bạn không có quyền xoá vận đơn này.",
                                                                                            billCode.ToUpper());
                }
                else
                {
                    context.BillOfLadings.Remove(billToDelete);
                    context.SaveChanges();
                    HttpContext.Session[SiteConfig.TransferMessageSession] = string.Format("Vận đơn mã {0} đã được xoá.", billCode.ToUpper());
                }
            }
            return RedirectToAction("Bills");
        }

        public ActionResult ExportBill(int page = 1)
        {
            var customers = new List<Customer>();
            var filter = Session[SiteConfig.BillFilterCriteriaSession] == null ?
                        new BillFilterCriteria
                        {
                            SentDateFrom = DateTime.Now.AddDays(-7),
                            SentDateTo = DateTime.Now
                        } :              
                        (BillFilterCriteria)Session[SiteConfig.BillFilterCriteriaSession];
            Session[SiteConfig.BillFilterCriteriaSession] = filter;     
            var bills = SearchBill(filter, out customers);            
            var paging = PagingHelper.Paging(bills.Count, 30, page);
            ViewBag.Paging = paging;
            ViewBag.Customers = new SelectList(customers, "Id", "CustomerName", filter.CustomerId);
            return View(bills.GetRange(paging.StartRecord - 1, paging.EndRecord + 1 - paging.StartRecord));
        }

        [HttpPost]
        public ActionResult ExportBill(BillFilterCriteria filterCriteria, string SentDateFrom, string SentDateTo,
                                       string ReceivedDateFrom, string ReceivedDateTo, string btnExport, int page = 1)
        {
            filterCriteria.SentDateFrom = SetDateTime(SentDateFrom);
            filterCriteria.SentDateTo = SetDateTime(SentDateTo);
            filterCriteria.ReceivedDateFrom = SetDateTime(ReceivedDateFrom);
            filterCriteria.ReceivedDateTo = SetDateTime(ReceivedDateTo);
            Session[SiteConfig.BillFilterCriteriaSession] = filterCriteria;
            var customers = new List<Customer>();
            var bills = SearchBill(filterCriteria, out customers);
            ViewBag.Customers = new SelectList(customers, "Id", "CustomerName", filterCriteria.CustomerId);
            
            if (btnExport != null)
            {
                return ExportBillList(bills, filterCriteria);
            }
            var paging = PagingHelper.Paging(bills.Count, 30, page);
            ViewBag.Paging = paging;
            return View(bills.GetRange(paging.StartRecord - 1, paging.EndRecord + 1 - paging.StartRecord));
        }

        private void GetSenderInfoByCustomer(BillOfLading bill)
        {
            using(var context = new WebsiteDBEntities())
            {
               if(bill.Customer.HasValue)
               {
                   try
                   {
                       var customer = context.Customers.SingleOrDefault(i => i.Id == bill.Customer);
                       bill.Sender = customer.ContactPerson;
                       bill.SenderAddress = customer.CustomerAddress;
                       bill.SenderContactName = customer.ContactPerson;
                       bill.SenderTel = customer.ContactNumber;
                   }
                   catch(Exception e)
                   {
                       throw new Exception("Không tìm thấy thông tin khách hàng.");
                   }
               }
            }
        }

        private List<BillOfLading> SearchBill(BillFilterCriteria filterCriteria, out List<Customer> customers)
        {
            using (var context = new WebsiteDBEntities())
            {
                var bills = context.BillOfLadings
                                   .Include("Customer1")
                                   .Where(i =>  (filterCriteria.BillId == null || i.BillCode.Contains(filterCriteria.BillId)) &&
                                                (filterCriteria.SentDateFrom == null || i.SentDate >= filterCriteria.SentDateFrom) &&
                                                (filterCriteria.SentDateTo == null || i.SentDate <= filterCriteria.SentDateTo) &&
                                                (filterCriteria.ReceivedDateFrom == null || i.ReceivedDate >= filterCriteria.ReceivedDateFrom) &&
                                                (filterCriteria.ReceivedDateTo == null || i.ReceivedDate <= filterCriteria.ReceivedDateTo) &&
                                                (filterCriteria.CustomerId == null || i.Customer.Value == filterCriteria.CustomerId.Value))
                                   .OrderByDescending(i => i.SentDate).ToList();
                filterCriteria.CustomerName = filterCriteria.CustomerId.HasValue ?
                                                context.Customers
                                                       .SingleOrDefault(i => i.Id == filterCriteria.CustomerId.Value)
                                                       .CustomerName :
                                                string.Empty;
                customers = context.Customers.ToList();
                return bills;
            }
        }

        private DateTime? SetDateTime(string dateValue)
        {
            if(string.IsNullOrEmpty(dateValue)){
                return null;
            }
            return DateTimeHelper.ConvertDatetimeStringToDateTime(dateValue, DateTimeHelper.dateFormat.ddMMyyyy, '-');
        }

        private BinaryResult ExportBillList(IEnumerable<BillOfLading> bills, BillFilterCriteria filterCriteria)
        {
            var result = new MemoryStream();
            var template = new ExcelFile();
            template.LoadXlsx(Server.MapPath("~/Content/Documents/BillList.xlsx"), XlsxOptions.None);
            var worksheet = template.Worksheets[0];

            worksheet.Cells["B3"] .Value = filterCriteria.CustomerId.HasValue ? filterCriteria.CustomerName : string.Empty;
            worksheet.Cells["B4"].Value = filterCriteria.SentDateFrom.HasValue ? 
                                          filterCriteria.SentDateFrom.Value.ToString("dd/MM/yyyy") : string.Empty;
            worksheet.Cells["B5"].Value = filterCriteria.SentDateTo.HasValue ? 
                                          filterCriteria.SentDateTo.Value.ToString("dd/MM/yyyy") : string.Empty;

            var startRow = 7;
            var currentRow = startRow;
            var startCol = 0;
            foreach (var bill in bills)
            {
                worksheet.Rows[currentRow].Cells[startCol].Value = bill.BillCode;
                worksheet.Rows[currentRow].Cells[startCol + 1].Value = bill.Sender.ToUpperInvariant();
                worksheet.Rows[currentRow].Cells[startCol + 2].Value = bill.SentDate.ToString("dd/MM/yyyy");
                worksheet.Rows[currentRow].Cells[startCol + 3].Value = string.IsNullOrEmpty(bill.Receiver) ?
                                                                       string.Empty : bill.Receiver.ToUpperInvariant();
                worksheet.Rows[currentRow].Cells[startCol + 4].Value = bill.ReceivedDate.HasValue ?
                                                                       bill.ReceivedDate.Value.ToString("dd/MM/yyyy HH:mm") :
                                                                       string.Empty;
                worksheet.Rows[currentRow].Cells[startCol + 5].Value = string.IsNullOrEmpty(bill.ActualReceiver) ?
                                                                       string.Empty : bill.ActualReceiver.ToUpperInvariant();
                worksheet.Rows[currentRow].Cells[startCol + 6].Value = string.IsNullOrEmpty(bill.DeliveryStaff) ?
                                                                       string.Empty : bill.DeliveryStaff.ToUpperInvariant();
                currentRow++;
            }

            CellBorder(worksheet, startRow, currentRow - 1, startCol, 6);
            template.SaveXlsx(result);
            return new BinaryResult
            {
                IsAttachment = true,
                Data = result.ToArray(),
                ContentType = "application/vnd.ms-excel",
                FileName = string.Format("BillList-{0}.xlsx", DateTime.Now.ToString("dd-MM-yyyy"))
            };
        }

        private void ConvertBillDateTime(BillOfLading bill, string sentDate, string receivedDate, string retransfer1, string retransfer2)
        {
            bill.SentDate = DateTimeHelper.ConvertDatetimeStringToDateTime(sentDate, DateTimeHelper.dateFormat.ddMMyyyy, '-');

            if(!string.IsNullOrWhiteSpace(receivedDate))
            {
                bill.ReceivedDate = DateTimeHelper.ConvertDatetimeStringToDateTime(receivedDate, DateTimeHelper.dateFormat.ddMMMyyyyHHmm, '-');
            }

            if (!string.IsNullOrWhiteSpace(retransfer1))
            {
                bill.RetransferTime1 = DateTimeHelper.ConvertDatetimeStringToDateTime(retransfer1, DateTimeHelper.dateFormat.ddMMyyyy, '-');
            }

            if (!string.IsNullOrWhiteSpace(retransfer2))
            {
                bill.RetransferTime2 = DateTimeHelper.ConvertDatetimeStringToDateTime(retransfer2, DateTimeHelper.dateFormat.ddMMyyyy, '-');
            }
        }

        private void CellBorder(ExcelWorksheet sheet, int startRow, int endRow, int startCol, int endCol)
        {
            for (int x = startRow; x <= endRow; x++)
            {
                for (int y = startCol; y <= endCol; y++)
                {
                    sheet.Rows[x].Cells[y].Style.Borders.SetBorders(MultipleBorders.Outside, System.Drawing.Color.Black, LineStyle.Thin);
                }
            }

        }

        private void CheckBillStatus(BillOfLading bill, string currentStatus)
        {
            if (bill.IsReadOnly)
                throw new Exception("Vận đơn này đã được khoá. Yêu cầu không được thay đổi thông tin vận đơn.");

            switch (bill.LadingStatus)
            {
                case SiteConfig.ProcessStatus:
                    //if (currentStatus == SiteConfig.Retransfer1Status || currentStatus == SiteConfig.Retransfer2Status)
                    //    throw new Exception("Chuyển đổi trạng thái vận đơn không hợp lệ");

                    bill.ReceivedDate = null;
                    bill.RetransferTime1 = null;
                    bill.RetransferTime2 = null;
                    bill.ActualReceiver = null;
                    return;

                case SiteConfig.Retransfer1Status:
                    //if (currentStatus != SiteConfig.ProcessStatus)
                    //    throw new Exception("Chuyển đổi trạng thái vận đơn không hợp lệ");

                    if(bill.RetransferTime1 == null) 
                        throw new Exception("Yêu cầu nhập thời gian phát lại lần 1.");

                    if (bill.RetransferTime1 <= bill.SentDate)
                        throw new Exception("Thời gian phát lại không hợp lý. Yêu cầu kiểm tra lại thông tin nhập.");

                    bill.ReceivedDate = null;
                    bill.RetransferTime2 = null;
                    bill.ActualReceiver = null;
                    return;

                case SiteConfig.Retransfer2Status:
                    if(bill.RetransferTime1 == null || bill.RetransferTime2 == null) 
                        throw new Exception("Yêu cầu nhập thời gian phát lại.");

                    if (bill.RetransferTime2 <= bill.RetransferTime1 || bill.RetransferTime1 <= bill.SentDate)
                        throw new Exception("Thời gian phát lại không hợp lý. Yêu cầu kiểm tra lại thông tin nhập.");

                    bill.ReceivedDate = null;
                    bill.ActualReceiver = null;
                    return;
                case SiteConfig.CompleteStatus:
                    if (bill.ReceivedDate == null || string.IsNullOrEmpty(bill.ActualReceiver))
                        throw new Exception("Yêu cầu nhập ngày nhận và tên người ký nhận");

                    if(currentStatus == SiteConfig.Retransfer1Status && 
                       (bill.RetransferTime1 == null || bill.RetransferTime1 <= bill.SentDate || bill.ReceivedDate <= bill.RetransferTime1))
                        throw new Exception("Thời gian nhận không hợp lý. Yêu cầu kiểm tra lại thông tin nhập.");

                    if (currentStatus == SiteConfig.Retransfer2Status &&
                       (bill.RetransferTime1 == null || bill.RetransferTime2 == null || bill.RetransferTime1 <= bill.SentDate ||
                       bill.RetransferTime2 <= bill.RetransferTime1 || bill.ReceivedDate <= bill.RetransferTime2))
                        throw new Exception("Thời gian nhận không hợp lý. Yêu cầu kiểm tra lại thông tin nhập.");

                    if (bill.ReceivedDate <= bill.SentDate ||
                        (bill.RetransferTime1.HasValue && (bill.RetransferTime1 <= bill.SentDate || bill.ReceivedDate <= bill.RetransferTime1)) ||
                        (bill.RetransferTime2.HasValue && (bill.RetransferTime1 == null || bill.RetransferTime2 < bill.RetransferTime1 ||
                        bill.RetransferTime1 <= bill.SentDate || bill.ReceivedDate <= bill.RetransferTime2)))
                            throw new Exception("Thời gian nhận không hợp lý. Yêu cầu kiểm tra lại thông tin nhập.");

                    bill.IsReadOnly = true;
                    return;

                case SiteConfig.FailStatus:
                    if (string.IsNullOrWhiteSpace(bill.FailNote))
                        throw new Exception("Yêu cầu nhập lý do không chuyển được");

                    bill.IsReadOnly = true;
                    return;

                default:
                    throw new Exception("Tình trạng phát không hợp lệ");
            }
        }
    }
}
