using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class OverrideQuoteSettingsForExistingTenants : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
insert into AbpSettings
(TenantId, Name, Value, CreationTime)
select t.Id, 'App.Quote.GeneralTermsAndConditions', 'GENERAL TERMS AND CONDITIONS

PRICES AND TERMS
Prices are based on the terms and conditions set forth on page 1 of this Quotation, of which these General Terms and Conditions form a part, the terms and conditions stated in Customer’s Application for Business Credit, and, if applicable, any terms and conditions relating to the delivery or shipment of materials by truck or other means which are provided by {CompanyName} to Customer in addition to this Quotation (each, an “{CompanyName} Sales Document”, and collectively, the ""{CompanyName} Sales Documents""). If not specifically stated otherwise, payment terms are Net 30. Prices are available only to the customer specifically named therein, and are only for the quantities mentioned in such Quotation or Sales Order plus or minus 10 % of such quantities.A delinquency charge of 1.5 % per month, or such maximum rate allowable by applicable law, will be imposed upon all invoice amounts delinquent, before and after judgment.Customer’s contract with {CompanyName} regarding the sale by {CompanyName} to Customer of the materials listed in this Quotation is subject to the terms and conditions set forth in the {CompanyName} Sales Documents.Prices reflect Customer''s acceptance of materials at the quoted plant based upon gradation analysis performed and reported by the respective plant(s) quality control personnel.Any penalties that result from in place sampling shall be the full responsibility of Customer.

{CompanyNameUpperCase} SALES DOCUMENTS GOVERN THE RIGHTS AND OBLIGATIONS OF THE PARTIES
All sales of materials shall be subject to the terms and conditions set forth in the {CompanyName} Sales Documents. Customer’s receipt of materials shall constitute acceptance of this Quotation and the {CompanyName} Sales Documents. Any terms or conditions of a purchase order issued by Customer either before or after this Quotation that are inconsistent with the terms and conditions of the {CompanyName} Sales Documents shall be null and void.

SHIPMENT AND DELIVERY
All taxes applicable to the sale or delivery of materials that are not paid directly by Customer will be added to the sales price, invoiced to and paid by Customer, unless Customer provides {CompanyName} with satisfactory evidence of exemption from same.Shipment will be in accordance with Customer’s reasonable instructions or, if none, then by whatever means {CompanyName} shall deem practicable. The quantities of material delivered to Customer shall be conclusively presumed to be the quantities shown on the tickets produced from a certified weigh scale at the respective quarry or sales yard.

CREDIT AND DEFAULT
{CompanyName} shall have no obligation to ship or deliver except upon its determination prior to each shipment or delivery that Customer is worthy of the credit to be extended and is not in default upon any obligation to {CompanyName}. Upon default, Customer agrees to pay all of {CompanyName}’s collection expenses, including attorneys’ fees.
{CompanyNameUpperCase} SHALL IN NO EVENT BE RESPONSIBLE FOR ANY INCIDENTAL OR CONSEQUENTIAL DAMAGES CAUSED BY NONCOMPLIANCE OF THE MATERIAL WITH SPECIFICATIONS, DEFECTS IN THE MATERIAL OR ANY EVENT ARISING OUT OF OR RELATED TO THIS QUOTATION. {CompanyName} shall have no liability for delay or failure to make shipments, or delivery, as a result of strikes, labor problems, severe weather conditions, casualty, mechanical breakdown or other conditions beyond {CompanyName}’s control. {CompanyName}’s liability and Customer’s exclusive remedy for any cause of action arising out of the Quotation shall be the replacement of the materials or refund of the purchase price.

INDEMNIFICATION: Customer shall defend, indemnify and hold harmless {CompanyName}, its representatives, members, designees, officers, directors, shareholders, employees, agents, successors and assigns(“Indemnified Parties”), from and against all claims, lawsuits, demands, damages, losses, judgments, settlements and expenses, including but not limited to attorney’s and consultant fees and expenses, arising out of, allegedly arising out of, resulting from or allegedly resulting from, in whole or in part, the sale, handling, delivery, storage or processing of the materials or any acts or omissions of Customer and any of its employees, agents or any entity working for Customer.This indemnity and defense obligation is valid regardless of whether or not such claim, damage, loss or expense is caused in part by {CompanyName}; however, Customer shall not be obligated to indemnify and defend {CompanyName} for claims found to be due to the sole negligence or willful misconduct of {CompanyName}. {CompanyName} shall be entitled to recover all attorney fees and costs incurred in enforcing this indemnity obligation;

CHANGE OF TERMS
{CompanyName} may change the price, quantity, and/or any other terms and conditions of this Quotation upon 30 days’ notice to Customer.

APPLICABLE LAW
The laws of the state in which materials are delivered shall apply to the sale of all materials subject hereto.

LIMITED WARRANTY AND WARRANTY DISCLAIMER
{CompanyNameUpperCase} EXCLUDES ALL WARRANTIES OF MERCHANTABILITY AND FITNESS FOR PARTICULAR PURPOSE AND ALL OTHER WARRANTIES, EXPRESS OR IMPLIED.
In addition, {CompanyName} makes no warranty whatsoever with respect to specific gravity, absorption, whether the material is innocuous, non-deleterious, or non-reactive, or whether the material is in conformance with any plans, other specifications, regulations, ordinances, statutes, or other standards applicable to Customer’s job or to the material as used by Customer.',
getDate() from AbpTenants t
left join AbpSettings s on s.TenantId = t.Id and s.UserId is null and s.Name = 'App.Quote.GeneralTermsAndConditions'
where s.Id is null
");

            migrationBuilder.Sql(@"
insert into AbpSettings
(TenantId, Name, Value, CreationTime)
select t.Id, 'App.Project.DefaultNotes', 'Loads must be maintained in compliance with all Federal, State, and Local DOT regulations for weight and containment of material.
Rental charge begins when dispatched en route to project and continues until truck is signed out by customer at the project and/or dump-site.
To be delivered at approximately 18 to 22 tons per load. Please add applicable sales tax.
Fuel surcharge will be calculated from a base cost of {BaseFuelCost} per gallon on-road diesel fuel (http://www.eia.gov/petroleum/gasdiesel/)
Add 2.5% to each item listed for every additional 15 cent increase in the cost per gallon over the base rate.
(Surcharge will not be added until fuel cost exceed {BaseFuelCost+15} per gallon)',
getDate() from AbpTenants t
left join AbpSettings s on s.TenantId = t.Id and s.UserId is null and s.Name = 'App.Project.DefaultNotes'
where s.Id is null
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
