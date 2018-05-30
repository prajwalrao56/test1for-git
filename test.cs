using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;
using Telerik.WinControls;
using CRS2011DeskApp.App_Code;
using Telerik.WinControls.UI;
using System.Collections;
using System.Reflection;

namespace CRS2011DeskApp.Cargo
{
    public partial class frmLuggageBooking : Telerik.WinControls.UI.RadForm, ICargoBooking
    {
        //bool is_hans_config, is_shatabdi_config;
        bool is_insurance_company, is_direct_freight_company, is_crossing_company, is_stax_company;

        private int xPos = 0;
        private int xPos1 = 0;
        public int intCompanyID = 0;
        public int intBranchID = 0;
        public int intUserID = 0;
        ArrayList arrListCntrl;
        public DataSet dsRights = null;
        public Boolean AllowRateChange = true;
        public Boolean IsClicked = false;
        public Boolean toGetTransfer = false;
        public Boolean toGetShort = false;

        public Boolean IsOrangeTypeDisplay = false;

        public int intAllowFareChange = 0;
        public int intLimitOnFareChange = 0;
        public int intLimitOnFareChangePer = 0;

        public int intAllowUpdate = 0;
        public int intAllowUpdateAll = 0;
        public int intAllowUpdateOtherBranches = 0;
        public int intAllowHamaliEdit = 0;
        public int intMaxHamaliPerUnit = 0;
        public int intMaxHamaliPerLR = 0;

        public int intBranchCityID = 0;
        public int intShownBookingID = 0;

        public DataTable dtConsignType = null;
        public DataTable dtGriSourse = null;
        //Boolean IsEscape = false;

        decimal dcmTaxPCT = 4.5M;
        decimal dcmMinTaxAmount = 750;

        public Color objCurrColor = Color.FromArgb(255, 213, 125);

        public Color objColorPaid = Color.FromArgb(213, 255, 213);
        public Color objColorToPay = Color.FromArgb(255, 224, 192);
        public Color objColorFOC = Color.FromArgb(255, 255, 192);
        public Color objColorVPP = Color.FromArgb(255, 213, 230);
        public Color objColorOnAcc = Color.FromArgb(192, 192, 230);
        public Color objColorWhite = Color.FromArgb(255, 255, 255);

        public DataTable dtRateMaster = null;
        int celIndex = 0;
        int rowIndex = 0;

        Boolean ToLoad = false;
        Boolean IsValueChanged = false;
        int intIsManual = 0;
        int intIsOnAccount = 0;

        public DataTable dtFromCities = null;
        public DataTable dtToCities = null;
        public DataTable dtExCities = null;

        public string strSelectedLRNo = "";
        int GSTType = 1; //1 -> STA, 2 -> Hans 
        public delegate void dlgShowLastLRNO();

        int intIDProofTypeID = 0;
        string strIDProofNo = "";
        string strSenderImageName = "";
        string strIDProofImageName = "";
        int intIsBillNo = 0;
        int intIsEwayBillNo = 0;
        string strLaserPrinterName = "";
        string strStickerPrinterName = "";
        DateTime dtEwayBillStartDate;
        DateTime dtEwayBillEndDate;

        public delegate void dlgUploadImgtos3();

        public frmLuggageBooking()
        {
            InitializeComponent();
            intCompanyID = Common.GetCompanyID();
            intBranchID = Common.GetBranchID();
            intUserID = Common.GetUserID();
            intBranchCityID = Common.GetBranchCityID();

            is_insurance_company = is_direct_freight_company = is_crossing_company = is_stax_company = false;

            GetRights();
            GetUserDetails();
            CargoSettingConfig();

            dtEwayBillStartDate = Common.GetServerTime(intUserID, intCompanyID);
            dtEwayBillEndDate = Common.GetServerTime(intUserID, intCompanyID);

            if (intCompanyID == 11 || intCompanyID == 1688 || intCompanyID == 2776)
                is_insurance_company = true;
            else
                is_insurance_company = false;

            string strCompanyLikeOrange = App_Code.Cargo.CompanyDisplayLikOrange(intUserID, Common.GetLogID());
            if (strCompanyLikeOrange.Split('^')[0].Contains("||" + intCompanyID + "||"))
                IsOrangeTypeDisplay = true;

            Boolean IsDailyExpense = false;
            if (strCompanyLikeOrange.Split('^')[1].Contains("|" + intCompanyID + "|"))
                IsDailyExpense = true;

            radBDailyExpense.Visible = IsDailyExpense;

            this.KeyPreview = true;

            dtFromCities = GetCargoFromCities();
            dtToCities = GetCargoToCities();
            dtExCities = GetCargoExCities();

            FillBookingCity();
            radDDBookingCity.SelectedValue = intBranchCityID.ToString();

            lblCrossingCity.Visible = false;
            lblCrossingBranch.Visible = false;
            radDDCrossingCity.Visible = false;
            radDDCrossingBranch.Visible = false;

            FillServiceTaxPayer();

            FillDestCities();
            FillBookingBranch();

            FillCrossingCities();

            radDDBookingBranch.SelectedValue = intBranchID.ToString();

            FillModeOfPayment();

            FillDestinationBranch();

            fillDates();

            FillCrossingBranch();

            createDT();
            RemoveValuesFromControls();

            fillGrid();

            SetTabOrder();

            GetRateMaster();
            setPaymentMode();
            chkCrossing.Visible = false;
            radDDCStaxPaidBy.Visible = false;
            radDDCStaxPaidBy.Enabled = false;
            lblStaxPaidBy.Visible = false;
            chkInsurance.Visible = false;
            chkInsurance.Checked = false;

            if (Common.IsCompanyAdminUser() || Common.GetCompanyID() == 1)
            {
                btnChangeVehicle.Visible = true;
            }

            if (intCompanyID == 403)
            {
                chkCrossing.Visible = true;
            }

            if (is_stax_company)
            {
                radDDCStaxPaidBy.Visible = true;
                lblStaxPaidBy.Visible = true;
                radDDCStaxPaidBy.SelectedValue = 2;
            }
            else
                radDDCStaxPaidBy.SelectedValue = 1;

            if (is_insurance_company)
            {
                chkInsurance.Visible = true;
            }
            if (is_crossing_company)
            {
                chkCrossing.Visible = true;
            }

            if (is_direct_freight_company)
            {
                radGridConsignItems.Columns["Freight"].ReadOnly = false;
                radGridConsignItems.Columns["Rate"].IsVisible = false;
                if (intCompanyID == 168) //for royal-travles nagpur
                {
                    ((GridViewDecimalColumn)radGridConsignItems.Columns["Freight"]).Maximum = dcmMinTaxAmount; // 750;
                }
                else
                {
                    ((GridViewDecimalColumn)radGridConsignItems.Columns["Freight"]).Maximum = 9999999;
                }
            }
            else
            {
                radGridConsignItems.Columns["Freight"].ReadOnly = true;
            }

            chkSenderMobileGetData.Enabled = true;
            chkRecMobileGetData.Enabled = true;
        }


        private void FillServiceTaxPayer()
        {
            radDDCStaxPaidBy.DataSource = null;
            try
            {
                DataSet ds = App_Code.Cargo.GetServiceTaxPayer();
                radDDCStaxPaidBy.DisplayMember = "ServiceTaxPayer";
                radDDCStaxPaidBy.ValueMember = "STPID";
                radDDCStaxPaidBy.DataSource = ds.Tables[0];
            }
            catch (Exception ex)
            {
                radDDCStaxPaidBy.DataSource = null;
            }
        }


        public void GetRateMaster()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                if (intBranchCityID == 0)
                    return;

                DataSet dsRateMaster = App_Code.Cargo.GetRateMaster(intCompanyID, intBranchCityID, 0, 0, intBranchID, intUserID, 2, "Consignment", 0);

                if (dsRateMaster != null && dsRateMaster.Tables.Count > 0 && dsRateMaster.Tables[0].Rows.Count > 0)
                {
                    dtRateMaster = dsRateMaster.Tables[0].Copy();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        public void GetRights()
        {
            try
            {
                int intRoleID = Common.GetRoleIDFromCache();

                dsRights = App_Code.Cargo.CargoRoleRightsConfigListAll(intRoleID, intUserID);

                intAllowHamaliEdit = Convert.ToInt32(dsRights.Tables[1].Rows[0]["AllowHamaliEdit"]);

                if (intAllowHamaliEdit == 0)
                    txtHamaliChg.Enabled = false;
                else
                    txtHamaliChg.Enabled = true;

                intMaxHamaliPerUnit = Convert.ToInt32(dsRights.Tables[1].Rows[0]["MaxHamaliPerUnit"]);
                intMaxHamaliPerLR = Convert.ToInt32(dsRights.Tables[1].Rows[0]["MaxHamaliPerLR"]);

            }
            catch (Exception ex)
            {
                intAllowHamaliEdit = 0;
                intMaxHamaliPerUnit = 0;
                intMaxHamaliPerLR = 0;
            }
            finally
            {

            }
        }

        public frmLuggageBooking(Boolean _ToLoad)
            : this()
        {
            ToLoad = _ToLoad;
        }

        private void SetTabOrder()
        {
            try
            {
                arrListCntrl = new ArrayList();
                arrListCntrl.Add(radDDDestCity);
                arrListCntrl.Add(radDDDestBranch);
                arrListCntrl.Add(radDPayType);
                arrListCntrl.Add(txtManualLR);
                arrListCntrl.Add(chkIsPartySender);
                arrListCntrl.Add(radDPartySender);
                arrListCntrl.Add(txtNameSender);
                arrListCntrl.Add(txtMobileNo);
                arrListCntrl.Add(txtSenderAddress);
                arrListCntrl.Add(chkIsCollection);
                arrListCntrl.Add(radDCollectionType);
                arrListCntrl.Add(txtBillNo);
                //arrListCntrl.Add(radDDDDBill);
                //arrListCntrl.Add(radDDMMBill);
                //arrListCntrl.Add(radDDYYBill);
                arrListCntrl.Add(dtBillNo);
                arrListCntrl.Add(txtEwayBillNo);
                arrListCntrl.Add(radbtnEwayBillDate);
                arrListCntrl.Add(txtSenderGSTN);
                //arrListCntrl.Add(radDDCStaxPaidBy);
                arrListCntrl.Add(chkIsPartyReceiver);
                arrListCntrl.Add(radDPartyReceiver);
                arrListCntrl.Add(txtNameReceiver);
                arrListCntrl.Add(txtMobileNoReceiver);
                arrListCntrl.Add(txtAddressReceiver);
                arrListCntrl.Add(txtReceiverGSTN);
                arrListCntrl.Add(chkIsDelivery);
                arrListCntrl.Add(raddDeliveryType);
                arrListCntrl.Add(chkInsurance);

                arrListCntrl.Add(txtDeliveryChg);
                arrListCntrl.Add(txtCollectionChg);
                arrListCntrl.Add(txtCartage);
                arrListCntrl.Add(txtDocumentChg);
                arrListCntrl.Add(txtInsurance);
                arrListCntrl.Add(txtHamaliChg);
                arrListCntrl.Add(txtServiceTax);
                arrListCntrl.Add(radDModeofPayment);
                arrListCntrl.Add(txtComment);

                arrListCntrl.Add(radBSave);


                object[] arrFromCntrl = arrListCntrl.ToArray();
                for (int i = 0; i < arrFromCntrl.Length; i++)
                {
                    ((Control)arrFromCntrl[i]).KeyDown += new KeyEventHandler(SetFocus_KeyUp);
                }
            }
            catch (Exception ex)
            { }
        }

        private void SetFocus_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (((Control)sender).Name != radGridConsignItems.Name)
                {
                    string strCurrentControl = ((Control)sender).Name;
                    if (IsValid(((Control)sender)))
                    {
                        GetTabOrder(strCurrentControl);
                    }
                }

            }
        }

        private void GetTabOrder(string strCurrentControl)
        {
            try
            {
                object[] arrFromCntrl = arrListCntrl.ToArray();

                bool blnContains = false;
                int i = 0;
                for (i = 0; i < arrFromCntrl.Length; i++)
                {
                    if (((Control)arrFromCntrl[i]).Name == strCurrentControl)
                    {
                        blnContains = true;
                        break;
                    }
                }
                if (blnContains)
                {
                    for (int j = i + 1; j < arrFromCntrl.Length; j++)
                    {
                        //if (radBtnSearch.Name == strCurrentControl && txtCode.Text != "")
                        //{
                        //    j++;
                        //}

                        if (((Control)arrFromCntrl[j]).Enabled && ((Control)arrFromCntrl[j]).Visible)
                        {
                            ((Control)arrFromCntrl[j]).Focus();
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

        }

        private bool IsValid(object eventSource)
        {
            string name = "EventValidating";

            Type targetType = eventSource.GetType();

            do
            {
                FieldInfo[] fields = targetType.GetFields(
                     BindingFlags.Static |
                     BindingFlags.Instance |
                     BindingFlags.NonPublic);

                foreach (FieldInfo field in fields)
                {
                    if (field.Name == name)
                    {
                        EventHandlerList eventHandlers = ((EventHandlerList)(eventSource.GetType().GetProperty("Events",
                            (BindingFlags.FlattenHierarchy |
                            (BindingFlags.NonPublic | BindingFlags.Instance))).GetValue(eventSource, null)));

                        Delegate d = eventHandlers[field.GetValue(eventSource)];

                        if ((!(d == null)))
                        {
                            Delegate[] subscribers = d.GetInvocationList();

                            // ok we found the validation event,  let's get the event method and call it
                            foreach (Delegate d1 in subscribers)
                            {
                                // create the parameters
                                object sender = eventSource;
                                CancelEventArgs eventArgs = new CancelEventArgs();
                                eventArgs.Cancel = false;
                                object[] parameters = new object[2];
                                parameters[0] = sender;
                                parameters[1] = eventArgs;
                                // call the method
                                d1.DynamicInvoke(parameters);
                                // if the validation failed we need to return that failure
                                if (eventArgs.Cancel)
                                    return false;
                                else
                                    return true;
                            }
                        }
                    }
                }

                targetType = targetType.BaseType;

            } while (targetType != null);

            return true;
        }

        public void fillPayType()
        {
            try
            {
                DataTable dtBooking = null;
                DataTable dtDispatch = null;
                DataTable dtRefund = null;
                DataTable dtReceipt = null;
                DataTable dtDelivery = null;
                DataTable dtBranchTransfer = null;
                DataTable dtBranchReceipt = null;
                DataTable dtRateChange = null;

                grpBxSenderInfo.BackColor = objColorWhite;
                grpBxReceiverInfo.BackColor = objColorWhite;
                grpBxConsignmentItems.BackColor = objColorWhite;
                grpBXPayment.BackColor = objColorWhite;
                grpCharges.BackColor = objColorWhite;

                if (dsRights != null && dsRights.Tables.Count > 0 && dsRights.Tables[0].Rows.Count > 0)
                {
                    dtBooking = new DataView(dsRights.Tables[0], "BookingRightsID=1", "", DataViewRowState.CurrentRows).ToTable();
                    dtRefund = new DataView(dsRights.Tables[0], "BookingRightsID=2", "", DataViewRowState.CurrentRows).ToTable();
                    dtDispatch = new DataView(dsRights.Tables[0], "BookingRightsID=3", "", DataViewRowState.CurrentRows).ToTable();
                    dtReceipt = new DataView(dsRights.Tables[0], "BookingRightsID=4", "", DataViewRowState.CurrentRows).ToTable();
                    dtDelivery = new DataView(dsRights.Tables[0], "BookingRightsID=6", "", DataViewRowState.CurrentRows).ToTable();
                    dtBranchTransfer = new DataView(dsRights.Tables[0], "BookingRightsID=8", "", DataViewRowState.CurrentRows).ToTable();
                    dtBranchReceipt = new DataView(dsRights.Tables[0], "BookingRightsID=9", "", DataViewRowState.CurrentRows).ToTable();
                    //dtRateChange = new DataView(dsRights.Tables[0], "BookingRightsID=7", "", DataViewRowState.CurrentRows).ToTable();
                }

                if (dtBooking.Rows[0]["Allowed"].ToString() == "0")
                    radBSave.Enabled = false;

                if (dtRefund.Rows[0]["Allowed"].ToString() == "0")
                    radBRefund.Enabled = false;

                if (dtDispatch.Rows[0]["Allowed"].ToString() == "0")
                    radButton10.Enabled = false;

                if (dtReceipt.Rows[0]["Allowed"].ToString() == "0")
                    radButton11.Enabled = false;

                if (dtDelivery.Rows[0]["Allowed"].ToString() == "0")
                {
                    radBDelivery.Enabled = false;
                    radBDeliveryMemo.Enabled = false;
                }

                if (dtBranchTransfer.Rows[0]["Allowed"].ToString() == "0")
                    radBBranchTransfer.Enabled = false;

                if (dtBranchReceipt.Rows[0]["Allowed"].ToString() == "0")
                    radBBranchReceipt.Enabled = false;

                if (dsRights.Tables.Count > 1 && dsRights.Tables[1].Rows.Count > 0)
                {
                    intAllowFareChange = Convert.ToInt32(dsRights.Tables[1].Rows[0]["AllowFareChange"]);
                    if (intAllowFareChange == 0)
                    {
                        radGridConsignItems.Columns["Rate"].ReadOnly = true;
                        AllowRateChange = false;
                    }
                    else if (intAllowFareChange == 1)
                    {
                        intLimitOnFareChange = Convert.ToInt32(dsRights.Tables[1].Rows[0]["LimitOnFareChange"]);
                        intLimitOnFareChangePer = Convert.ToInt32(dsRights.Tables[1].Rows[0]["LimitOnFareChangePer"]);
                    }
                    else if (intAllowFareChange == 2)
                    {
                        intLimitOnFareChange = 0;
                        intLimitOnFareChangePer = 0;
                    }

                    intAllowUpdate = Convert.ToInt32(dsRights.Tables[1].Rows[0]["AllowUpdate"]);
                    intAllowUpdateAll = Convert.ToInt32(dsRights.Tables[1].Rows[0]["AllowUpdateAll"]);
                    intAllowUpdateOtherBranches = Convert.ToInt32(dsRights.Tables[1].Rows[0]["AllowUpdateOtherBranches"]);
                }

                DataTable dtPayType = new DataTable();
                DataColumn dc = new DataColumn("PayType");
                dc.DataType = System.Type.GetType("System.String");
                dtPayType.Columns.Add(dc);

                dc = new DataColumn("PayTypeID");
                dc.DataType = System.Type.GetType("System.String");
                dtPayType.Columns.Add(dc);

                DataRow dr = dtPayType.NewRow();

                DataTable dtFromCtForPayType = new DataView(dtFromCities, "CityID = " + intBranchCityID, "", DataViewRowState.CurrentRows).ToTable();
                DataTable dtToCtForPayType = new DataView(dtToCities, "CityID = " + Convert.ToInt32(radDDDestCity.SelectedValue), "", DataViewRowState.CurrentRows).ToTable();

                if (dtBooking.Rows[0]["Paid"].ToString() != "0" &&
                    dtFromCtForPayType.Rows[0]["IsPaid"].ToString() != "0" &&
                     dtToCtForPayType.Rows[0]["IsPaid"].ToString() != "0")
                {
                    dr["PayType"] = "PAID";
                    dr["PayTypeID"] = "1";
                    dtPayType.Rows.Add(dr);
                }

                if (dtBooking.Rows[0]["TOPAY"].ToString() != "0" &&
                    dtFromCtForPayType.Rows[0]["IsToPay"].ToString() != "0" &&
                    dtToCtForPayType.Rows[0]["IsToPay"].ToString() != "0")
                {
                    dr = dtPayType.NewRow();
                    dr["PayType"] = "TO-PAY";
                    dr["PayTypeID"] = "2";
                    dtPayType.Rows.Add(dr);
                }

                if (dtBooking.Rows[0]["FOC"].ToString() != "0" &&
                    dtFromCtForPayType.Rows[0]["IsFOC"].ToString() != "0" &&
                    dtToCtForPayType.Rows[0]["IsFOC"].ToString() != "0")
                {
                    dr = dtPayType.NewRow();
                    dr["PayType"] = "FOC";
                    dr["PayTypeID"] = "3";
                    dtPayType.Rows.Add(dr);
                }

                if (dtBooking.Rows[0]["COD"].ToString() != "0" &&
                    dtFromCtForPayType.Rows[0]["IsCOD"].ToString() != "0" &&
                    dtToCtForPayType.Rows[0]["IsCOD"].ToString() != "0")
                {
                    dr = dtPayType.NewRow();
                    dr["PayType"] = "COD";
                    dr["PayTypeID"] = "4";
                    dtPayType.Rows.Add(dr);
                }

                if (dtBooking.Rows[0]["OnAcc"].ToString() != "0" &&
                    intIsOnAccount == 1 &&
                    dtFromCtForPayType.Rows[0]["OnAcc"].ToString() != "0" &&
                    dtToCtForPayType.Rows[0]["OnAcc"].ToString() != "0")
                {
                    dr = dtPayType.NewRow();
                    dr["PayType"] = "On-Account";
                    dr["PayTypeID"] = "5";
                    dtPayType.Rows.Add(dr);
                }

                if (dtBooking.Rows[0]["Paid"].ToString() != "0" &&
                    intIsManual == 1 &&
                    dtFromCtForPayType.Rows[0]["IsPaid"].ToString() != "0" &&
                    dtToCtForPayType.Rows[0]["IsPaid"].ToString() != "0")
                {
                    dr = dtPayType.NewRow();
                    dr["PayType"] = "PAID(Manual)";
                    dr["PayTypeID"] = "6";
                    dtPayType.Rows.Add(dr);
                }

                if (dtBooking.Rows[0]["TOPAY"].ToString() != "0" &&
                    intIsManual == 1 &&
                    dtFromCtForPayType.Rows[0]["IsToPay"].ToString() != "0" &&
                    dtToCtForPayType.Rows[0]["IsToPay"].ToString() != "0")
                {
                    dr = dtPayType.NewRow();
                    dr["PayType"] = "TO-PAY(Manual)";
                    dr["PayTypeID"] = "7";
                    dtPayType.Rows.Add(dr);
                }

                radDPayType.DisplayMember = "PayType";
                radDPayType.ValueMember = "PayTypeID";
                radDPayType.DataSource = dtPayType;
            }
            catch (Exception ex)
            {

            }
        }

        public void FillPartiesforBookingCity()
        {
            try
            {
                DataTable dtPartyFrom = new DataTable();

                DataTable dtParty = Common.GetLuggageParties("B");

                if (dtParty != null && dtParty.Rows.Count > 0)
                {
                    dtPartyFrom = new DataView(dtParty, "CityID=" + radDDBookingCity.SelectedValue.ToString() + "AND MappedBranchID=0", "", DataViewRowState.CurrentRows).ToTable();

                    //dsPartyFrom = App_Code.Cargo.PartyMasterListAll(0, intCompanyID, "", "", Convert.ToInt32(radDDBookingCity.SelectedValue), -1, -1, 0);

                    radDPartySender.DisplayMember = "Party";
                    radDPartySender.ValueMember = "PartyDetails";
                    radDPartySender.DataSource = dtPartyFrom;
                }
                else
                    radDPartySender.DataSource = null;

            }
            catch (Exception ex)
            {

            }
        }

        public void FillPartiesforDestCity()
        {
            try
            {
                DataTable dtPartyTo = new DataTable();

                DataTable dtParty = Common.GetLuggageParties("B");

                if (dtParty != null && dtParty.Rows.Count > 0)
                {
                    dtPartyTo = new DataView(dtParty, "CityID=" + radDDDestCity.SelectedValue.ToString() + "AND MappedBranchID=0", "", DataViewRowState.CurrentRows).ToTable();

                    radDPartyReceiver.DisplayMember = "Party";
                    radDPartyReceiver.ValueMember = "PartyDetails";
                    radDPartyReceiver.DataSource = dtPartyTo;
                }
                else
                    radDPartyReceiver.DataSource = null;
            }
            catch (Exception ex)
            {

            }
        }

        private void frmLuggageBooking_Load(object sender, EventArgs e)
        {
            if (ToLoad)
                tableLayoutPanel1.Visible = true;

            string marqeeAllowed = App_Code.Cargo.GetMarqeeAllowedCompanyID(intUserID);
            string strCompID = "||" + intCompanyID + "||";

            if (marqeeAllowed.Contains(strCompID))
            {
                xPos = panel1.Width;
                GetTransferAlerts();
                GetShortReceiptAlerts();
                timer1.Start();
            }

            //xPos1 = panel1.Width;

            //timer2.Start();
        }

        public void createDT()
        {
            try
            {
                //dtConsignType = new DataTable();
                //DataColumn dc = new DataColumn("ConsignmentType");
                //dc.DataType = System.Type.GetType("System.String");
                //dtConsignType.Columns.Add(dc);

                //dc = new DataColumn("ConsignmentTypeID");
                //dc.DataType = System.Type.GetType("System.String");
                //dtConsignType.Columns.Add(dc);

                //DataRow dr = dtConsignType.NewRow();
                //dr["ConsignmentType"] = "--Select--";
                //dr["ConsignmentTypeID"] = "0";
                //dtConsignType.Rows.Add(dr);

                //dr = dtConsignType.NewRow();
                //dr["ConsignmentType"] = "Box";
                //dr["ConsignmentTypeID"] = "1";
                //dtConsignType.Rows.Add(dr);

                //dr = dtConsignType.NewRow();
                //dr["ConsignmentType"] = "Box2";
                //dr["ConsignmentTypeID"] = "2";
                //dtConsignType.Rows.Add(dr);

                dtConsignType = new DataTable();

                dtConsignType = Common.GetLuggageConsignTypes("B");
                //DataSet dsConsignType = App_Code.Cargo.ConsignmentSubTypeListAll(0, intCompanyID, 0, "", 1, 0);

                //ToComment
                //dtConsignType = dsConsignType.Tables[0];

                DataRow dr = null;

                if (dtConsignType != null && dtConsignType.Rows.Count > 0)
                {
                    dr = dtConsignType.NewRow();
                    dr["SubType"] = "--Select--";
                    dr["ConsignmentSubTypeID"] = "0";
                    dtConsignType.Rows.InsertAt(dr, 0);
                }
                else
                {
                    dtConsignType = new DataTable();

                    DataColumn dc1 = new DataColumn("SubType");
                    dc1.DataType = System.Type.GetType("System.String");
                    dtConsignType.Columns.Add(dc1);

                    dc1 = new DataColumn("ConsignmentSubTypeID");
                    dc1.DataType = System.Type.GetType("System.String");
                    dtConsignType.Columns.Add(dc1);

                    dr = dtConsignType.NewRow();
                    dr["SubType"] = "--Select--";
                    dr["ConsignmentSubTypeID"] = "0";
                    dtConsignType.Rows.InsertAt(dr, 0);
                }


                dtGriSourse = new DataTable();
                DataColumn dc = new DataColumn("ConsignmentTypeID");
                dc.DataType = System.Type.GetType("System.String");
                dtGriSourse.Columns.Add(dc);

                dc = new DataColumn("Description");
                dc.DataType = System.Type.GetType("System.String");
                dtGriSourse.Columns.Add(dc);

                dc = new DataColumn("Qty");
                dc.DataType = System.Type.GetType("System.Int32");
                dtGriSourse.Columns.Add(dc);

                dc = new DataColumn("Goodsvalue");
                dc.DataType = System.Type.GetType("System.String");
                dtGriSourse.Columns.Add(dc);

                dc = new DataColumn("Weight");
                dc.DataType = System.Type.GetType("System.Double");
                dtGriSourse.Columns.Add(dc);

                dc = new DataColumn("Rate");
                dc.DataType = System.Type.GetType("System.String");
                dtGriSourse.Columns.Add(dc);

                dc = new DataColumn("Freight");
                dc.DataType = System.Type.GetType("System.Double");
                dtGriSourse.Columns.Add(dc);
            }
            catch (Exception ex)
            { }
        }

        public void AddNewRow(string ConsignmentType, string Description, string Qty, string GoodsValue, string Rate, string Freight, double Weight)
        {
            radGridConsignItems.DataSource = null;

            DataRow drw = dtGriSourse.NewRow();
            drw["ConsignmentTypeID"] = ConsignmentType;
            drw["Description"] = Description;
            drw["Qty"] = Qty;
            drw["Goodsvalue"] = GoodsValue;
            drw["Weight"] = Weight;
            drw["Rate"] = Rate;
            drw["Freight"] = Freight;
            dtGriSourse.Rows.Add(drw);

            //this.radGridConsignItems.MasterTemplate.AllowAddNewRow = true;
        }

        public void fillGrid()
        {
            try
            {
                radGridConsignItems.AutoGenerateColumns = false;
                radGridConsignItems.DataSource = dtGriSourse;

                ((Telerik.WinControls.UI.GridViewComboBoxColumn)radGridConsignItems.Columns["ConsignmentType"]).DataSource = dtConsignType;
                ((Telerik.WinControls.UI.GridViewComboBoxColumn)radGridConsignItems.Columns["ConsignmentType"]).DisplayMember = "SubType";
                ((Telerik.WinControls.UI.GridViewComboBoxColumn)radGridConsignItems.Columns["ConsignmentType"]).ValueMember = "ConsignmentSubTypeID";
            }
            catch (Exception ex)
            { }
        }

        public DataTable GetCargoFromCities()
        {
            DataSet ds = null;
            DataTable dt = null;
            try
            {
                ds = App_Code.Cargo.GetCargoCities(1, intCompanyID, intBranchID, intUserID);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    dt = ds.Tables[0];
                }
            }
            catch (Exception ex)
            { }
            return dt;
        }

        public DataTable GetCargoToCities()
        {
            DataSet ds = null;
            DataTable dt = null;
            try
            {
                ds = App_Code.Cargo.GetCargoCities(2, intCompanyID, intBranchID, intUserID);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    dt = ds.Tables[0];
                }
            }
            catch (Exception ex)
            { }
            return dt;
        }

        public DataTable GetCargoExCities()
        {
            DataSet ds = null;
            DataTable dt = null;
            try
            {
                ds = App_Code.Cargo.GetCargoCities(3, intCompanyID, intBranchID, intUserID);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    dt = ds.Tables[0];
                }
            }
            catch (Exception ex)
            { }
            return dt;
        }

        public void FillBookingCity()
        {
            try
            {

                DataSet ds = new DataSet();
                DataTable dtFrmCities = new DataTable();

                //dtFrmCities = Common.GetCompanyCities("B");
                dtFrmCities = App_Code.Cargo.CargoCompanyCitiesListAll(intUserID, intCompanyID, 0);

                DataView dv = new DataView(dtFrmCities, "", "CityName ASC", DataViewRowState.CurrentRows);

                DataTable dtFrmCts = dv.ToTable();

                int intActualCity = new DataView(dtFrmCts, "CityID=" + intBranchCityID, "", DataViewRowState.CurrentRows).ToTable().Rows.Count;

                if (intActualCity <= 0)
                    dtFrmCts = null;

                radDDBookingCity.DisplayMember = "CityName";
                radDDBookingCity.ValueMember = "CityID";
                radDDBookingCity.DataSource = dtFrmCts;

            }
            catch (Exception ex)
            {

            }
        }

        public void FillDestCities()
        {
            try
            {

                DataSet ds = new DataSet();
                DataTable dtDestCities = new DataTable();

                //dtDestCities = Common.GetCompanyCities("B");
                dtDestCities = App_Code.Cargo.CargoCompanyCitiesListAll(intUserID, intCompanyID, 0);

                DataTable dtToCts = new DataView(dtDestCities, "CityID <>" + intBranchCityID.ToString(), "CityName ASC", DataViewRowState.CurrentRows).ToTable();

                List<DataRow> rowsToDelete = new List<DataRow>();
                foreach (DataRow dr in dtToCts.Rows)
                {
                    DataTable dtTemp = new DataView(dtToCities, "CityID=" + dr["CityID"] + "AND IsPaid = 0 AND IsToPay = 0 AND IsFoc = 0 AND IsCOD = 0 AND OnAcc = 0", "", DataViewRowState.CurrentRows).ToTable();

                    if (dtTemp != null && dtTemp.Rows.Count > 0)
                    {
                        rowsToDelete.Add(dr);
                    }
                }

                foreach (var dataRow in rowsToDelete)
                {
                    dtToCts.Rows.Remove(dataRow);
                }

                radDDDestCity.DisplayMember = "CityName";
                radDDDestCity.ValueMember = "CityID";
                radDDDestCity.DataSource = dtToCts;

            }
            catch (Exception ex)
            {

            }
        }

        public void FillCrossingCities()
        {
            DataTable dtDestCities = new DataTable();

            //dtDestCities = Common.GetCompanyCities("B");
            dtDestCities = App_Code.Cargo.CargoCompanyCitiesListAll(intUserID, intCompanyID, 0);

            try
            {
                DataTable dtToCts = new DataView(dtDestCities, "CityID <>" + intBranchCityID.ToString() + " and CityID <> " + radDDDestCity.SelectedValue.ToString(), "CityName ASC", DataViewRowState.CurrentRows).ToTable();

                List<DataRow> rowsToDelete = new List<DataRow>();
                foreach (DataRow dr in dtToCts.Rows)
                {
                    DataTable dtTemp = new DataView(dtExCities, "CityID=" + dr["CityID"] + " AND IsPaid = 0 AND IsToPay = 0 AND IsFoc = 0 AND IsCOD = 0 AND OnAcc = 0", "", DataViewRowState.CurrentRows).ToTable();

                    if (dtTemp != null && dtTemp.Rows.Count > 0)
                    {
                        rowsToDelete.Add(dr);
                    }
                }

                foreach (var dataRow in rowsToDelete)
                {
                    dtToCts.Rows.Remove(dataRow);
                }

                radDDCrossingCity.DisplayMember = "CityName";
                radDDCrossingCity.ValueMember = "CityID";
                radDDCrossingCity.DataSource = dtToCts;

            }
            catch (Exception ex)
            {

            }
        }


        public void FillBookingBranch()
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dtBranch = new DataTable();
                DataTable dtTempBranch = new DataTable();
                DataTable dtLuggageBranch = new DataTable();

                radDDBookingBranch.DataSource = null;

                dtBranch = Common.GetBranches("B");

                dtTempBranch = dtBranch.Copy();

                dtLuggageBranch = new DataView(dtTempBranch, "BranchTypeID in ('2','3')", "BranchName ASC", DataViewRowState.CurrentRows).ToTable();

                int intBkgBrnchActual = new DataView(dtLuggageBranch, "BranchID=" + Common.GetBranchID(), "", DataViewRowState.CurrentRows).ToTable().Rows.Count;

                if (intBkgBrnchActual <= 0)
                    dtLuggageBranch = null;

                radDDBookingBranch.DisplayMember = "BranchName";
                radDDBookingBranch.ValueMember = "BranchID";
                radDDBookingBranch.DataSource = dtLuggageBranch;
            }
            catch (Exception ex)
            {

            }
        }

        public void FillDestinationBranch()
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dtBranch = new DataTable();
                DataTable dtTempBranch1 = new DataTable();

                radDDDestBranch.DataSource = null;

                //dtBranch = Common.GetBranches("B",true);
                dtBranch = Common.GetBranchesCargoShared("B", Convert.ToInt32("0" + radDDDestCity.SelectedValue.ToString()), Convert.ToInt32("0"+radDDBookingBranch.SelectedValue.ToString()));

                if (dtBranch != null)
                {
                    dtTempBranch1 = dtBranch.Copy();

                    DataTable dtDestBranches = new DataView(dtTempBranch1, "BranchTypeID in ('2','3') AND CargoHasDelivery = 1 AND CityID=" + radDDDestCity.SelectedValue.ToString(), "BranchName asc", DataViewRowState.CurrentRows).ToTable();

                    radDDDestBranch.DisplayMember = "BranchName";
                    radDDDestBranch.ValueMember = "BranchID";

                    if (dtDestBranches.Rows.Count > 1)
                    {
                        DataRow dr = dtDestBranches.NewRow();
                        dr["BranchName"] = "--SELECT--";
                        dr["BranchID"] = "0";

                        dtDestBranches.Rows.InsertAt(dr, 0);
                    }

                    radDDDestBranch.DataSource = dtDestBranches;

                    //FillCrossingBranch();

                    //Pradeep : 2018-01-16 : Hardcode requested by Jain for below two branches and same has been done for dest city as well 
                    try
                    {
                        if (blnIsOfflineMode)
                        {
                            if (intCompanyID == 805)
                                radDDDestBranch.SelectedValue = "3902";
                            if (intCompanyID == 2921)
                                radDDDestBranch.SelectedValue = "22465";

                            if (intCompanyID == 805 || intCompanyID == 2921)
                                radDDDestBranch.Enabled = false;
                        }
                    }
                    catch (Exception ex)
                    { }

                }

            }
            catch (Exception ex)
            {
            }
        }

        private void FillCrossingBranch()
        {
            try
            {
                DataTable dtBranch = new DataTable();
                radDDCrossingBranch.DataSource = null;
                //dtBranch = Common.GetBranches("B");
                dtBranch = Common.GetBranchesCargoShared("B", Convert.ToInt32("0" + radDDCrossingCity.SelectedValue.ToString()),-1);

                DataTable dtTempBranch1 = dtBranch.Copy();

                DataTable dtDestBranches = new DataView(dtTempBranch1, "BranchTypeID in ('2','3') AND CityID=" + radDDCrossingCity.SelectedValue.ToString(), "BranchName asc", DataViewRowState.CurrentRows).ToTable();

                radDDCrossingBranch.DisplayMember = "BranchName";
                radDDCrossingBranch.ValueMember = "BranchID";
                radDDCrossingBranch.DataSource = dtDestBranches;
            }
            catch (Exception)
            {
            }

        }

        private void fillDates()
        {
            List<clsMonth> objMonth = new List<clsMonth>();
            //List<clsMonth> objMonth2 = new List<clsMonth>();

            System.Globalization.DateTimeFormatInfo mfi = new System.Globalization.DateTimeFormatInfo();

            for (int i = 1; i <= 31; i++)
            {
                radDDDD.Items.Add(i.ToString());
            }
            for (int i = 1; i <= 12; i++)
            {
                objMonth.Add(new clsMonth(i.ToString(), mfi.GetAbbreviatedMonthName(i)));
            }
            radDDMM.DisplayMember = "MonthName";
            radDDMM.ValueMember = "MonthID";
            radDDMM.DataSource = objMonth;

            int intYear = DateTime.Now.Year;
            radDDYY.Items.Add((intYear - 1).ToString());
            radDDYY.Items.Add(intYear.ToString());
            radDDYY.Items.Add((intYear + 1).ToString());

            radDDDD.Text = DateTime.Now.Day.ToString();
            radDDMM.SelectedValue = DateTime.Now.Month.ToString();
            radDDYY.Text = DateTime.Now.Year.ToString();
        }

        public void FillModeOfPayment()
        {
            try
            {
                DataTable dtModes = new DataTable();

                DataTable dtMode = new DataTable();
                DataColumn dc = new DataColumn("Mode");
                dc.DataType = System.Type.GetType("System.String");
                dtMode.Columns.Add(dc);

                dc = new DataColumn("ModeID");
                dc.DataType = System.Type.GetType("System.String");
                dtMode.Columns.Add(dc);

                DataRow dr = dtMode.NewRow();
                dr["Mode"] = "CASH";
                dr["ModeID"] = "1";
                dtMode.Rows.Add(dr);

                dr = dtMode.NewRow();
                dr["Mode"] = "CREDIT";
                dr["ModeID"] = "2";
                dtMode.Rows.Add(dr);

                radDModeofPayment.DisplayMember = "Mode";
                radDModeofPayment.ValueMember = "ModeID";
                radDModeofPayment.DataSource = dtMode;

            }
            catch (Exception ex)
            {

            }
        }

        public void CargoSettingConfig()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                DataSet dsCargoTax = new DataSet();

                dsCargoTax = App_Code.Cargo.GetCargoCompanyTax(intCompanyID);
                if (dsCargoTax != null && dsCargoTax.Tables.Count > 0 && dsCargoTax.Tables[0].Rows.Count > 0)
                {
                    dcmTaxPCT = Convert.ToDecimal(dsCargoTax.Tables[0].Rows[0]["TaxPct"].ToString());
                    dcmMinTaxAmount = Convert.ToDecimal(dsCargoTax.Tables[0].Rows[0]["MinTaxAmount"].ToString());
                }
                else
                {
                    dcmTaxPCT = 4.5M;
                    dcmMinTaxAmount = 750;
                }

                DataSet dsCorgoSettings = new DataSet();

                dsCorgoSettings = App_Code.Cargo.GetCargoCompanySettings(intCompanyID, intUserID);

                if (dsCorgoSettings != null && dsCorgoSettings.Tables.Count > 0 && dsCorgoSettings.Tables[0].Rows.Count > 0)
                {
                    #region "Party"
                    if (dsCorgoSettings.Tables[0].Rows[0]["IsParty"].ToString() == "0" || Convert.ToInt32(dsRights.Tables[1].Rows[0]["AllowPartyBooking"]) == 0)
                    {
                        lblIsParty.Visible = false;
                        lblIsParty2.Visible = false;
                        chkIsPartyReceiver.Visible = false;
                        chkIsPartySender.Visible = false;
                    }
                    else
                    {
                        lblIsParty.Visible = true;
                        lblIsParty2.Visible = true;
                        chkIsPartyReceiver.Visible = true;
                        chkIsPartySender.Visible = true;
                    }
                    #endregion

                    int intIsFreight = 0, intIsCollChg = 0, intIsCartage = 0,
                        intIsDocument = 0, intIsInsurance = 0, intIsServiceTax = 0;
                    //intIsBillNo = 0, intIsEwayBillNo = 0;

                    #region "OnAccountAndManual"
                    intIsManual = Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["IsManual"]);
                    intIsOnAccount = Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["IsOnAccount"]);
                    #endregion

                    #region "BillNo"
                    intIsBillNo = Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["IsBillNo"]);
                    intIsEwayBillNo = Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["HasEwayBillNo"]);

                    SetVisibilityOfBillAndEwayBillNo(intIsBillNo, intIsEwayBillNo);

                    //if (intIsBillNo == 0)
                    //{
                    //    lblBillNo.Visible = false;
                    //    txtBillNo.Visible = false;
                    //    //radDDDDBill.Visible = false;
                    //    //radDDMMBill.Visible = false;
                    //    //radDDYYBill.Visible = false;
                    //    dtBillNo.Visible = false;
                    //}
                    //else
                    //{
                    //    lblBillNo.Visible = true;
                    //    txtBillNo.Visible = true;
                    //    //radDDDDBill.Visible = true;
                    //    //radDDMMBill.Visible = true;
                    //    //radDDYYBill.Visible = true;
                    //    dtBillNo.Visible = true;
                    //}


                    #endregion

                    #region "Delivery"
                    //intIsDelivery = Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["IsDelivery"]);
                    //if (intIsDelivery == 0)
                    //{
                    //    chkIsDelivery.Visible = false;
                    //    lblIsDelivery.Visible = false;
                    //    raddDeliveryType.Visible = false;
                    //    txtDeliveryChg.Text = "0";
                    //    lblDeliveryChg.Visible = false;
                    //    txtDeliveryChg.Visible = false;
                    //}
                    //else
                    //{
                    //    chkIsDelivery.Checked = false;

                    //    lblIsDelivery.Visible = true;
                    //    chkIsDelivery.Visible = true;
                    //    lblDeliveryChg.Visible = false;
                    //    txtDeliveryChg.Visible = false;
                    //}
                    #endregion

                    #region "CollChg"
                    intIsCollChg = Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["IsCollChg"]);
                    if (intIsCollChg == 0)
                    {
                        //chkIsCollection.Visible = false;
                        //lblIsCollection.Visible = false;
                        //radDCollectionType.Visible = false;
                        txtCollectionChg.Text = "0";
                        lblCollChgBx.Visible = false;
                        txtCollectionChg.Visible = false;
                    }
                    else
                    {
                        //chkIsCollection.Checked = false;
                        //lblIsCollection.Visible = true;
                        //chkIsCollection.Visible = true;
                        lblCollChgBx.Visible = true;
                        txtCollectionChg.Visible = true;
                    }
                    #endregion

                    #region "Cartage"
                    intIsCartage = Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["IsCartage"]);
                    if (intIsCartage == 0)
                    {
                        txtCartage.Text = "0";
                        lblCartageBx.Visible = false;
                        txtCartage.Visible = false;
                    }
                    else
                    {
                        lblCartageBx.Visible = true;
                        txtCartage.Visible = true;
                    }
                    #endregion

                    #region "DocChg"
                    intIsDocument = Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["IsDocument"]);
                    if (intIsDocument == 0)
                    {
                        txtDocumentChg.Text = "0";
                        lblDocChgBx.Visible = false;
                        txtDocumentChg.Visible = false;
                    }
                    else
                    {
                        lblDocChgBx.Visible = true;
                        txtDocumentChg.Visible = true;
                    }
                    #endregion

                    #region "Insurance"
                    intIsInsurance = Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["IsInsurance"]);
                    if (intIsInsurance == 0)
                    {
                        txtInsurance.Text = "0";
                        lblInsuranceBx.Visible = false;
                        txtInsurance.Visible = false;
                    }
                    else
                    {
                        if (IsOrangeTypeDisplay)
                            lblInsuranceBx.Text = "Door-Del.Chg";
                        lblInsuranceBx.Visible = true;
                        txtInsurance.Visible = true;
                    }

                    if (is_insurance_company)
                    {
                        lblInsuranceBx.Visible = true;
                        txtInsurance.Visible = true;
                        txtInsurance.Enabled = false;
                    }
                    #endregion

                    #region "ServiceTax"
                    intIsServiceTax = Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["IsServiceTax"]);
                    if (intIsServiceTax == 0)
                    {
                        txtServiceTax.Text = "0";
                        lblServiceTxBx.Visible = false;
                        txtServiceTax.Visible = false;
                        is_stax_company = false;
                    }
                    else
                    {
                        //if (IsOrangeTypeDisplay)
                        //    lblServiceTxBx.Text = "Hamali Chg";

                        lblServiceTxBx.Visible = true;
                        txtServiceTax.Visible = true;
                        is_stax_company = true;
                    }
                    #endregion

                    #region "Freight"
                    intIsFreight = Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["IsFreight"]);

                    if (intIsFreight == 1 || (intIsCollChg == 1 || intIsCartage == 1 || intIsDocument == 1 || intIsInsurance == 1 || intIsServiceTax == 1))
                    {
                        lblFreightTxtBx.Visible = true;
                        txtFreightChg.Visible = true;
                    }
                    else
                    {
                        lblFreightTxtBx.Visible = false;
                        txtFreightChg.Visible = false;
                    }
                    #endregion

                    #region "Description"
                    int intIsDescription = Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["IsDescription"]);

                    if (intIsDescription == 0)
                        radGridConsignItems.Columns[1].IsVisible = false;

                    #endregion

                    #region "GoodsValue"
                    int intIsGoodsValue = Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["IsGoodsValue"]);

                    if (intIsGoodsValue == 0)
                        radGridConsignItems.Columns[3].IsVisible = false;
                    #endregion

                    #region "Weight"
                    int intIsWeight = Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["IsWeight"]);

                    if (intIsWeight == 0)
                        radGridConsignItems.Columns[4].IsVisible = false;

                    if (intCompanyID == 438)
                    {
                        txtComment.Text = ".";
                        txtComment.Visible = false;
                    }
                    #endregion

                    #region "GSTType"
                    try
                    {
                        GSTType = Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["GstType"]);
                    }
                    catch (Exception ex)
                    {
                        GSTType = 2;
                    }
                    #endregion

                    #region "DirectFreightCompany & IsCrossingCompany"
                    try
                    {
                        is_direct_freight_company = (Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["IsDirectFreightCompany"]) == 1 ? true : false);
                        is_crossing_company = (Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["IsCrossingCompany"]) == 1 ? true : false);
                    }
                    catch (Exception ex)
                    { }
                    #endregion

                }

                //Per umesh's email for Anand Tourist: allow back-date booking (max 2 days - past)
                //Developer: config created in company-config table. but not ui provided as of now.
                if (intCompanyID == 1806)
                {
                    radDDDD.Enabled = true;
                    radDDMM.Enabled = true;
                    radDDYY.Enabled = true;

                    // handle backdate booking via Stored-Procedure.
                    //currently its a 2 day back-booking.. 
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void tableLayoutPanel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void radDropDownList1_SelectedIndexChanged(object sender, Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void radDDDestCity_SelectedIndexChanged(object sender, Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
        {
            try
            {
                if (radDDDestCity.SelectedIndex == -1)
                    return;

                this.Cursor = Cursors.WaitCursor;
                FillDestinationBranch();
                FillCrossingCities();
                FillPartiesforDestCity();
                fillPayType();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void radGridConsignItems_CellEndEdit(object sender, GridViewCellEventArgs e)
        {
            celIndex = e.ColumnIndex;
            rowIndex = e.RowIndex;

            if (!IsValueChanged && celIndex == 0)
                return;

            if (rowIndex == -1)
                return;

            string strRate = "";

            string ConsignmentSubType = radGridConsignItems.Rows[rowIndex].Cells["ConsignmentType"].Value.ToString();
            string Description = radGridConsignItems.Rows[rowIndex].Cells["Description"].Value.ToString();
            string Qty = radGridConsignItems.Rows[rowIndex].Cells["Qty"].Value.ToString();
            string Goodsvalue = radGridConsignItems.Rows[rowIndex].Cells["Goodsvalue"].Value.ToString();
            string Weight = radGridConsignItems.Rows[rowIndex].Cells["Weight"].Value.ToString();
            string Rate = radGridConsignItems.Rows[rowIndex].Cells["Rate"].Value.ToString();

            try
            {
                strRate = ConsignmentSubType.Split('-')[1];
            }
            catch (Exception ex)
            { }

            if (celIndex == 0)
            {
                if (radGridConsignItems.Columns[1].IsVisible)
                    radGridConsignItems.Rows[rowIndex].Cells[1].BeginEdit();
                else
                    radGridConsignItems.Rows[rowIndex].Cells[2].BeginEdit();

                string ConsignMentTypeID = "";
                try
                {
                    ConsignMentTypeID = ConsignmentSubType.Split('-')[0].Trim();
                    strRate = ConsignmentSubType.Split('-')[1].Trim();
                }
                catch (Exception ex)
                {
                    strRate = "";
                }


                if (dtRateMaster != null && dtRateMaster.Rows.Count > 0)
                {
                    DataTable dTRate = new DataView(dtRateMaster, "ConsignmentSubTypeID = " + ConsignMentTypeID
                                                                + " AND FromCityID = " + intBranchCityID
                                                                + " AND ToCityID = " + Convert.ToInt32(radDDDestCity.SelectedValue),
                                                        "", DataViewRowState.CurrentRows).ToTable();

                    if (dTRate != null && dTRate.Rows.Count > 0)
                    {
                        strRate = dTRate.Rows[0]["Rate"].ToString();
                    }
                }

                try
                {
                    if (strRate == "")
                    {
                        strRate = ConsignmentSubType.Split('-')[1];
                    }
                }
                catch (Exception ex)
                {
                    strRate = "0";
                }


                if (is_direct_freight_company)
                    radGridConsignItems.Rows[rowIndex].Cells["Rate"].Value = "0";
                else
                    radGridConsignItems.Rows[rowIndex].Cells["Rate"].Value = strRate;
            }
            else if (celIndex == 1)
            {
                if (radGridConsignItems.Columns[2].IsVisible)
                    radGridConsignItems.Rows[rowIndex].Cells[2].BeginEdit();
                else
                    radGridConsignItems.Rows[rowIndex].Cells[3].BeginEdit();
            }
            else if (celIndex == 2) //unit
            {
                if (radGridConsignItems.Columns[3].IsVisible)
                    radGridConsignItems.Rows[rowIndex].Cells[3].BeginEdit();
                else if (radGridConsignItems.Columns[4].IsVisible)
                    radGridConsignItems.Rows[rowIndex].Cells[4].BeginEdit();
                else
                    radGridConsignItems.Rows[rowIndex].Cells[5].BeginEdit();

                //Pradeep : 2017-09-29 : Hansh Requirement By Vikas.. Max Hamali charge should be 10*no. of units
                try
                {
                    if (!Common.IsMTPLAdminUser() && !Common.IsCompanyAdminUser() && !Common.IsBranchAdminUser() && (intAllowHamaliEdit != 2))
                    {
                        //if (intCompanyID == 11 || intCompanyID == 403 || intCompanyID == 227 || intCompanyID == 512 || intCompanyID == 690 || intCompanyID == 1186 || intCompanyID == 824 || intCompanyID == 2776 || intCompanyID == 1688 || intCompanyID == 1387)
                        //{
                        if (txtHamaliChg.Visible == true)
                        {
                            txtHamaliChg.Text = "0";
                        }
                        //}
                    }
                }
                catch (Exception ex)
                {
                    txtHamaliChg.Text = "0";
                }
            }
            else if (celIndex == 3)
            {
                if (radGridConsignItems.Columns[4].IsVisible)
                    radGridConsignItems.Rows[rowIndex].Cells[4].BeginEdit();
                else if (radGridConsignItems.Columns[5].IsVisible)
                    radGridConsignItems.Rows[rowIndex].Cells[5].BeginEdit();
                else
                    radGridConsignItems.Rows[rowIndex].Cells[6].BeginEdit();
                CalcInsurance();
            }
            else if (celIndex == 4 && AllowRateChange)
            {
                if (is_direct_freight_company)
                    radGridConsignItems.Rows[rowIndex].Cells[6].BeginEdit();
                else
                    radGridConsignItems.Rows[rowIndex].Cells[5].BeginEdit();
            }
            else if ((!AllowRateChange && celIndex == 4) || celIndex == 5 || celIndex == 6)
            {
                //if ((e.Row.Cells["Rate"].Value.ToString() == "" || e.Row.Cells["Rate"].Value.ToString() == "0") && radDPayType.SelectedValue.ToString() != "3")
                //{
                //    MessageBox.Show("Please Enter Rate.");
                //    radGridConsignItems.Rows[rowIndex].Cells[celIndex].BeginEdit();
                //}
                //else
                //{
                if (e.Row.Cells["Rate"].Value.ToString() != "0" && radDPayType.SelectedValue.ToString() == "3" && !is_direct_freight_company)
                {
                    MessageBox.Show("Rate should be zero as it is FOC consignment.");
                    e.Row.Cells["Rate"].Value = "0";
                    radGridConsignItems.Rows[rowIndex].Cells[celIndex].BeginEdit();
                    return;
                }

                if (!(Common.IsBranchAdminUser() || Common.IsMTPLAdminUser() || Common.IsCompanyAdminUser()))
                {
                    if (intAllowFareChange == 1)
                    {
                        double dblNewFare = Convert.ToDouble(e.Row.Cells["Rate"].Value.ToString());
                        double dblOldFare = Convert.ToDouble((strRate == "" ? "0" : strRate));

                        if (dblNewFare != dblOldFare)
                        {
                            double dblDiff = dblOldFare - dblNewFare;

                            if (dblDiff > 0)
                            {
                                if (intLimitOnFareChange > 0)
                                {
                                    if (dblDiff > intLimitOnFareChange)
                                    {
                                        MessageBox.Show("Insufficient Rights: Fare change is not allowed below Rs." + intLimitOnFareChange.ToString());
                                        radGridConsignItems.Rows[rowIndex].Cells["Rate"].Value = ConsignmentSubType.Split('-')[1];
                                        radGridConsignItems.Rows[rowIndex].Cells[celIndex].BeginEdit();
                                        return;
                                    }
                                }
                                else if (intLimitOnFareChangePer > 0)
                                {
                                    double dblMinNewFareLimit = (dblOldFare * intLimitOnFareChangePer) / 100;

                                    if (dblDiff > dblMinNewFareLimit)
                                    {
                                        MessageBox.Show("Insufficient Rights: Fare change is not allowed below  " + intLimitOnFareChangePer + "%");
                                        radGridConsignItems.Rows[rowIndex].Cells["Rate"].Value = ConsignmentSubType.Split('-')[1];
                                        radGridConsignItems.Rows[rowIndex].Cells[celIndex].BeginEdit();
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }

                string qty = e.Row.Cells["Qty"].Value.ToString();
                string rate = e.Row.Cells["Rate"].Value.ToString();


                string Freight = (Convert.ToInt32(qty) * Convert.ToDouble(rate)).ToString();


                try
                {
                    if (is_direct_freight_company && Convert.ToDouble(radGridConsignItems.Rows[rowIndex].Cells[6].Value) == 0)
                        radGridConsignItems.Rows[rowIndex].Cells[6].Value = Freight;
                    else if (!is_direct_freight_company)
                        radGridConsignItems.Rows[rowIndex].Cells[6].Value = Freight;
                }
                catch (Exception ex)
                {
                    radGridConsignItems.Rows[rowIndex].Cells[6].Value = "0";
                }


                //radGridConsignItems.Rows[rowIndex].Cells[5].BeginEdit();
                DataView dv = dtGriSourse.DefaultView;

                txtFreightChg.Text = dtGriSourse.Compute("SUM(Freight)", dv.RowFilter).ToString();

                if (is_direct_freight_company)
                {
                    if ((!AllowRateChange && celIndex == 4) || celIndex == 5)
                    {
                        radGridConsignItems.Rows[rowIndex].Cells[6].BeginEdit();
                        return;
                    }

                    //if (celIndex == 6 && Convert.ToDouble(radGridConsignItems.Rows[rowIndex].Cells[6].Value) > 750)
                    //{
                    //    MessageBox.Show("Freight Amount should not be greater than 750!", "Wrong", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //    radGridConsignItems.Rows[rowIndex].Cells[6].BeginEdit();
                    //    return;
                    //}
                }

                // Ktn : 2017/09/08 - Hans 10 RS Hamali per Unit
                //int ToCompanyID = 0;
                //try
                //{
                //    DataTable dtDestBranch = App_Code.Cargo.GetBranchDetails(Convert.ToInt32(radDDDestBranch.SelectedValue));
                //    if (dtDestBranch != null && dtDestBranch.Rows.Count > 0)
                //    {
                //        ToCompanyID = Convert.ToInt32(dtDestBranch.Rows[0]["CompanyID"].ToString());
                //    }
                //}
                //catch (Exception ex) { }

                //if (intCompanyID == 11 || ToCompanyID == 11)
                //{
                //    txtHamaliChg.Enabled = false;
                //    txtHamaliChg.Text = (Convert.ToInt32(qty) * 10).ToString();
                //}
                //else
                //    txtHamaliChg.Enabled = true;

                DialogResult dr;

                if ((intCompanyID == 633 || is_direct_freight_company) && intCompanyID != 1)
                    dr = DialogResult.No;
                else
                    dr = MessageBox.Show("Do you want to add another consignment?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if (dr == DialogResult.Yes)
                {
                    AddNewRow("0", "", "0", "0", "0", "0", 0);
                    fillGrid();

                    radGridConsignItems.Rows[rowIndex].IsSelected = false;
                    radGridConsignItems.Rows[rowIndex + 1].IsSelected = true;

                    radGridConsignItems.Rows[rowIndex].Cells[celIndex].IsSelected = false;
                    radGridConsignItems.Rows[rowIndex + 1].Cells[0].IsSelected = true;
                    radGridConsignItems.Rows[rowIndex + 1].Cells[0].BeginEdit();
                }
                else
                {
                    if (txtDeliveryChg.Enabled && txtDeliveryChg.Visible)
                        txtDeliveryChg.Focus();
                    else if (txtCollectionChg.Enabled && txtCollectionChg.Visible)
                        txtCollectionChg.Focus();
                    else if (txtCartage.Enabled && txtCartage.Visible)
                        txtCartage.Focus();
                    else if (txtDocumentChg.Enabled && txtDocumentChg.Visible)
                        txtDocumentChg.Focus();
                    else if (txtInsurance.Enabled && txtInsurance.Visible)
                        txtInsurance.Focus();
                    else if (txtHamaliChg.Enabled && txtHamaliChg.Visible)
                        txtHamaliChg.Focus();
                    else if (txtServiceTax.Enabled && txtServiceTax.Visible)
                        txtServiceTax.Focus();
                    else
                        txtComment.Focus();

                    txtServiceTax.Text = "0";
                    TotalAmount();
                }
                //}
                //}
            }
            IsValueChanged = false;
        }

        //private void radGridConsignItems_KeyPress(object sender, KeyPressEventArgs e)
        //{
        //    //MessageBox.Show(e.KeyChar.ToString());
        //    //return;
        //    if (e.KeyChar == 13)
        //    {
        //        if (rowIndex == -1)
        //            return;

        //        //radGridConsignItems.Rows[rowIndex].Cells[celIndex].EndEdit();
        //        if (celIndex == 0)
        //        {
        //                radGridConsignItems.Rows[rowIndex].Cells[1].BeginEdit();
        //        }
        //        else if (celIndex == 1)
        //        {
        //            radGridConsignItems.Rows[rowIndex].Cells[2].BeginEdit();
        //        }
        //        else if (celIndex == 2)
        //        {
        //                radGridConsignItems.Rows[rowIndex].Cells[3].BeginEdit();
        //        }
        //        else if (celIndex == 3)
        //        {
        //                radGridConsignItems.Rows[rowIndex].Cells[4].BeginEdit();
        //        }
        //        else if (celIndex == 4)
        //        {
        //            //if ((e.Row.Cells["Rate"].Value.ToString() == "" || e.Row.Cells["Rate"].Value.ToString() == "0") && radDPayType.SelectedValue.ToString() != "3")
        //            //{
        //            //    MessageBox.Show("Please Enter Rate.");
        //            //    radGridConsignItems.Rows[rowIndex].Cells[celIndex].BeginEdit();
        //            //}
        //            //else
        //            //{
        //            if (radGridConsignItems.Rows[rowIndex].Cells["Rate"].Value.ToString() != "0" && radDPayType.SelectedValue.ToString() == "3")
        //            {
        //                MessageBox.Show("Rate should be zero as it is FOC consignment.");
        //                radGridConsignItems.Rows[rowIndex].Cells["Rate"].Value = "0";
        //                radGridConsignItems.Rows[rowIndex].Cells[celIndex].BeginEdit();
        //                return;
        //            }

        //            string qty = radGridConsignItems.Rows[rowIndex].Cells["Qty"].Value.ToString();
        //            string rate = radGridConsignItems.Rows[rowIndex].Cells["Rate"].Value.ToString();

        //            string Freight = (Convert.ToInt32(qty) * Convert.ToInt32(rate)).ToString();

        //            radGridConsignItems.Rows[rowIndex].Cells[5].Value = Freight;
        //            //radGridConsignItems.Rows[rowIndex].Cells[5].BeginEdit();
        //            DataView dv = dtGriSourse.DefaultView;

        //            txtFreightChg.Text = dtGriSourse.Compute("SUM(Freight)", dv.RowFilter).ToString();

        //            DialogResult dr = MessageBox.Show("Do you want to add another consignment?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        //            if (dr == DialogResult.Yes)
        //            {
        //                AddNewRow("0", "", "0", "0", "0", "0");
        //                fillGrid();

        //                radGridConsignItems.Rows[rowIndex].IsSelected = false;
        //                radGridConsignItems.Rows[rowIndex + 1].IsSelected = true;

        //                radGridConsignItems.Rows[rowIndex].Cells[celIndex].IsSelected = false;
        //                radGridConsignItems.Rows[rowIndex + 1].Cells[0].IsSelected = true;
        //                radGridConsignItems.Rows[rowIndex + 1].Cells[0].BeginEdit();
        //            }
        //            else
        //            {
        //                if (!txtCollectionChg.Enabled)
        //                    txtComment.Focus();
        //                else
        //                    txtCollectionChg.Focus();

        //            }
        //            //}
        //            //}
        //        }
        //    }
        //}
        private void chkInsurance_CheckedChanged(object sender, EventArgs e)
        {
            CalcInsurance();
        }

        private void CalcInsurance()
        {
            txtInsurance.Text = "0";
            if (chkInsurance.Visible && chkInsurance.Checked)
            {
                decimal total_goods_value = 0;
                for (int i = 0; i < radGridConsignItems.Rows.Count; i++)
                {
                    total_goods_value += Convert.ToDecimal(radGridConsignItems.Rows[i].Cells[3].Value.ToString());
                }
                txtInsurance.Text = ((total_goods_value / 100) * (decimal)0.5).ToString();
            }
        }


        private void radGridConsignItems_Enter(object sender, EventArgs e)
        {
            radGridConsignItems.Rows[0].Cells[0].BeginEdit();
        }

        private void chkIsPartySender_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                Boolean chked = ((CheckBox)sender).Checked;

                if (intCompanyID == 168) //royal travels nagpur
                {
                    ((GridViewDecimalColumn)radGridConsignItems.Columns["Freight"]).Maximum = dcmMinTaxAmount; // 750;
                }

                if (chked)
                {
                    if (radDPayType.Items.Count > 0 && radDPayType.SelectedItem.Text.ToUpper() == "PAID")
                        radDModeofPayment.Enabled = true;
                    else
                        radDModeofPayment.Enabled = false;

                    FillPartiesforBookingCity();

                    if (radDPartySender.Items.Count > 0)
                    {
                        //FillPartiesforBookingCity();
                        radDPartySender.Visible = true;
                        radDPartySender.SelectedIndex = 0;
                        txtNameSender.Enabled = false;
                        txtMobileNo.Enabled = false;
                        chkSenderMobileGetData.Enabled = false;
                        SetSenderPartyValue();
                        radDPartySender.Focus();
                    }
                    else
                    {
                        chkIsPartySender.Checked = false;
                        chkSenderMobileGetData.Enabled = true;
                        MessageBox.Show("No party found.");
                        txtNameSender.Focus();
                    }
                }
                else
                {
                    radDModeofPayment.Enabled = false;
                    radDPartySender.Visible = false;
                    chkSenderMobileGetData.Enabled = true;
                    txtNameSender.Text = "";
                    txtMobileNo.Text = "";
                    txtNameSender.Enabled = true;
                    txtMobileNo.Enabled = true;
                    txtSenderGSTN.Text = "";
                    txtNameSender.Focus();
                }
                setPaymentMode();
            }
            catch (Exception ex)
            { }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void chkIsPartyReceiver_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                Boolean chked = ((CheckBox)sender).Checked;

                ((GridViewDecimalColumn)radGridConsignItems.Columns["Freight"]).Maximum = dcmMinTaxAmount; // 750;

                if (chked)
                {
                    FillPartiesforDestCity();
                    if (radDPartyReceiver.Items.Count > 0)
                    {

                        radDPartyReceiver.Visible = true;
                        radDPartyReceiver.SelectedIndex = 0;
                        txtNameReceiver.Enabled = false;
                        txtMobileNoReceiver.Enabled = false;
                        txtAddressReceiver.Enabled = false;
                        chkRecMobileGetData.Enabled = false;
                        SetReceiverPartyValue();
                        radDPartyReceiver.Focus();
                    }
                    else
                    {
                        MessageBox.Show("No party found.");
                        chkRecMobileGetData.Enabled = true;
                        txtNameReceiver.Focus();
                        chkIsPartyReceiver.Checked = false;
                    }
                }
                else
                {
                    radDPartyReceiver.Visible = false;
                    txtNameReceiver.Text = "";
                    txtMobileNoReceiver.Text = "";
                    txtAddressReceiver.Text = "";
                    txtNameReceiver.Enabled = true;
                    chkRecMobileGetData.Enabled = true;
                    txtMobileNoReceiver.Enabled = true;
                    txtAddressReceiver.Enabled = true;
                    txtReceiverGSTN.Text = "";
                    txtNameReceiver.Focus();
                }
                setPaymentMode();
            }
            catch (Exception ex)
            { }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void radDPartySender_SelectedIndexChanged(object sender, Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
        {
            SetSenderPartyValue();
        }

        private void radDPartyReceiver_SelectedIndexChanged(object sender, Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
        {
            SetReceiverPartyValue();
        }

        public void SetSenderPartyValue()
        {
            try
            {
                if (chkIsPartySender.Checked)
                {
                    string[] str = radDPartySender.SelectedValue.ToString().Split('^');

                    txtNameSender.Text = radDPartySender.SelectedItem.Text;
                    txtMobileNo.Text = str[1];

                    try
                    {
                        txtSenderGSTN.Text = str[5];
                    }
                    catch (Exception)
                    {
                        txtSenderGSTN.Text = "";
                    }

                    if (intCompanyID == 168)//royal travels nagpur
                    {
                        int intServiceTaxPayer = Convert.ToInt32(str[3]);

                        if (intServiceTaxPayer == 2 || intServiceTaxPayer == 3)
                            ((GridViewDecimalColumn)radGridConsignItems.Columns["Freight"]).Maximum = 9999999;
                        else
                            ((GridViewDecimalColumn)radGridConsignItems.Columns["Freight"]).Maximum = dcmMinTaxAmount; // 750;
                    }
                }
                else
                {
                    radDPartySender.Visible = false;
                    txtNameSender.Text = "";
                    txtMobileNo.Text = "";
                    txtNameSender.Enabled = true;
                    txtMobileNo.Enabled = true;
                }
            }
            catch
            {

            }
        }

        public void SetReceiverPartyValue()
        {
            try
            {
                if (chkIsPartyReceiver.Checked)
                {

                    string[] str = radDPartyReceiver.SelectedValue.ToString().Split('^');
                    //radDPartyReceiver.Visible = true;
                    txtNameReceiver.Text = radDPartyReceiver.SelectedItem.Text;
                    txtMobileNoReceiver.Text = str[1];
                    txtAddressReceiver.Text = str[2];

                    try
                    {
                        txtReceiverGSTN.Text = str[5];
                    }
                    catch (Exception)
                    {
                        txtReceiverGSTN.Text = "";
                    }

                    if (intCompanyID == 168)//royal travels nagpur
                    {
                        int intServiceTaxPayer = Convert.ToInt32(str[3]);

                        if (intServiceTaxPayer == 2 || intServiceTaxPayer == 3)
                            ((GridViewDecimalColumn)radGridConsignItems.Columns["Freight"]).Maximum = 9999999;
                        else
                            ((GridViewDecimalColumn)radGridConsignItems.Columns["Freight"]).Maximum = dcmMinTaxAmount; // 750;
                    }
                }
                else
                {
                    radDPartyReceiver.Visible = false;
                    txtNameReceiver.Text = "";
                    txtMobileNoReceiver.Text = "";
                    txtAddressReceiver.Text = "";
                    txtNameReceiver.Enabled = true;
                    txtMobileNoReceiver.Enabled = true;
                    txtAddressReceiver.Enabled = true;
                }
            }
            catch
            {

            }
        }

        private void radDPayType_SelectedIndexChanged(object sender, Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
        {
            try
            {
                //if (radDPayType.Items.Count > 0 && chkIsPartySender.Checked && radDPayType.SelectedItem.Text.ToUpper() == "PAID")
                //    radDModeofPayment.Enabled = true;
                //else
                //    radDModeofPayment.Enabled = false;

                //if (radDPayType.Items.Count > 0 && radDPayType.SelectedItem.Text.ToUpper() == "ON-ACCOUNT")
                //{
                //    radDModeofPayment.SelectedIndex = 1;
                //    radDModeofPayment.Enabled = false;
                //}

                txtCollectionChg.Enabled = true;
                txtDeliveryChg.Enabled = true;
                txtCartage.Enabled = true;
                txtDocumentChg.Enabled = true;
                txtInsurance.Enabled = true;

                if (intAllowHamaliEdit == 0)
                    txtHamaliChg.Enabled = false;
                else
                    txtHamaliChg.Enabled = true;

                txtServiceTax.Enabled = false;

                ShowHideManualLRBox(false);
                if (intCompanyID == 168)//royal travels nagpur
                    ((GridViewDecimalColumn)radGridConsignItems.Columns["Freight"]).Maximum = dcmMinTaxAmount; // 750;
                else
                    ((GridViewDecimalColumn)radGridConsignItems.Columns["Freight"]).Maximum = 9999999;

                if (radDPayType.SelectedValue.ToString() == "1" || radDPayType.SelectedValue.ToString() == "6")
                {
                    grpBxSenderInfo.BackColor = objColorPaid;
                    grpBxReceiverInfo.BackColor = objColorPaid;
                    grpBxConsignmentItems.BackColor = objColorPaid;
                    grpBXPayment.BackColor = objColorPaid;
                    grpCharges.BackColor = objColorPaid;

                    if (radDPayType.SelectedValue.ToString() == "6")
                    {
                        ShowHideManualLRBox(true);
                    }
                }
                else if (radDPayType.SelectedValue.ToString() == "2" || radDPayType.SelectedValue.ToString() == "7")
                {
                    grpBxSenderInfo.BackColor = objColorToPay;
                    grpBxReceiverInfo.BackColor = objColorToPay;
                    grpBxConsignmentItems.BackColor = objColorToPay;
                    grpBXPayment.BackColor = objColorToPay;
                    grpCharges.BackColor = objColorToPay;
                    if (radDPayType.SelectedValue.ToString() == "7")
                    {
                        ShowHideManualLRBox(true);
                    }
                }
                else if (radDPayType.SelectedValue.ToString() == "3")
                {
                    grpBxSenderInfo.BackColor = objColorFOC;
                    grpBxReceiverInfo.BackColor = objColorFOC;
                    grpBxConsignmentItems.BackColor = objColorFOC;
                    grpBXPayment.BackColor = objColorFOC;
                    grpCharges.BackColor = objColorFOC;

                    txtCollectionChg.Text = "0";
                    txtDeliveryChg.Text = "0";
                    txtCartage.Text = "0";
                    txtDocumentChg.Text = "0";
                    txtInsurance.Text = "0";
                    txtHamaliChg.Text = "0";
                    txtServiceTax.Text = "0";

                    txtCollectionChg.Enabled = false;
                    txtDeliveryChg.Enabled = false;
                    txtCartage.Enabled = false;
                    txtDocumentChg.Enabled = false;
                    txtInsurance.Enabled = false;

                    //Changed By Prateek: hamali to be captured during FOC. 03-feb-2014, chargo changes email from Krishna Singh
                    if (intCompanyID == 1247 || intCompanyID == 1 || intCompanyID == 1008 || intCompanyID == 977 || intCompanyID == 225 || intCompanyID == 1427 || intCompanyID == 422)
                    {
                        if (intAllowHamaliEdit == 0)
                            txtHamaliChg.Enabled = false;
                        else
                            txtHamaliChg.Enabled = true;
                    }
                    else
                    {
                        txtHamaliChg.Enabled = false;
                    }

                    txtServiceTax.Enabled = false;

                    ((GridViewDecimalColumn)radGridConsignItems.Columns["Freight"]).Maximum = 0;

                    for (int i = 0; i < radGridConsignItems.Rows.Count; i++)
                    {
                        try
                        {
                            radGridConsignItems.Rows[i].Cells["Freight"].Value = 0;
                        }
                        catch (Exception ex)
                        { }
                    }
                    //foreach (GridViewRow rw in radGridConsignItems.Rows)
                    //{
                    //}
                    //radGridConsignItems.Columns["Freight"].ReadOnly = true;
                }
                else if (radDPayType.SelectedValue.ToString() == "4")
                {
                    grpBxSenderInfo.BackColor = objColorVPP;
                    grpBxReceiverInfo.BackColor = objColorVPP;
                    grpBxConsignmentItems.BackColor = objColorVPP;
                    grpBXPayment.BackColor = objColorVPP;
                    grpCharges.BackColor = objColorVPP;
                }
                else if (radDPayType.SelectedValue.ToString() == "5")
                {
                    grpBxSenderInfo.BackColor = objColorOnAcc;
                    grpBxReceiverInfo.BackColor = objColorOnAcc;
                    grpBxConsignmentItems.BackColor = objColorOnAcc;
                    grpBXPayment.BackColor = objColorOnAcc;
                    grpCharges.BackColor = objColorOnAcc;
                }
                setPaymentMode();

                txtServiceTax.Text = "0";

                if (lblStaxPaidBy.Visible && radDDCStaxPaidBy.Visible && is_stax_company)
                {
                    if (GSTType == 1)
                    {
                        if (txtSenderGSTN.Text.Trim() != "" && txtReceiverGSTN.Text.Trim() != "")
                        {
                            if (radDPayType.SelectedValue.ToString() != "2")
                                radDDCStaxPaidBy.SelectedValue = 3; //Consigner
                            else
                                radDDCStaxPaidBy.SelectedValue = 4;  //Consignee
                        }
                    }
                    else
                    {
                        if (radDPayType.SelectedValue.ToString() != "2")
                        {
                            if (txtSenderGSTN.Text.Trim() == "")
                                radDDCStaxPaidBy.SelectedValue = 2; //Transporter
                            else
                                radDDCStaxPaidBy.SelectedValue = 3;  //Consigner
                        }
                        else
                        {
                            if (txtReceiverGSTN.Text.Trim() == "")
                                radDDCStaxPaidBy.SelectedValue = 2; //Transporter
                            else
                                radDDCStaxPaidBy.SelectedValue = 4;  //Consignee
                        }

                        TotalAmount();
                    }
                }

            }
            catch (Exception ex)
            { }
        }

        public void ShowHideManualLRBox(Boolean ToShow)
        {
            if (radBSave.Text == "Update")
            {
                lblManualLR.Visible = false;
                txtManualLR.Visible = false;

                tblLPManualLR.ColumnStyles[0].Width = 300;
                tblLPManualLR.ColumnStyles[1].Width = 0;
            }
            else
            {
                lblManualLR.Visible = ToShow;
                txtManualLR.Visible = ToShow;

                if (ToShow)
                {
                    tblLPManualLR.ColumnStyles[0].Width = 122;
                    tblLPManualLR.ColumnStyles[1].Width = 45;
                }
                else
                {
                    tblLPManualLR.ColumnStyles[0].Width = 300;
                    tblLPManualLR.ColumnStyles[1].Width = 0;
                }
            }
        }
        private void txtMobileNo_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = Common.AllowNumeric(e);
        }
        private void txtMobileNoReceiver_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = Common.AllowNumeric(e);
        }
        private void txtAddressReceiver_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if (chkIsDelivery.Visible)
                    chkIsDelivery.Focus();
                else
                    radGridConsignItems.Focus();
            }
        }
        private void txtCollectionChg_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = Common.AllowNumeric(e);
        }
        private void txtCartage_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = Common.AllowNumeric(e);
        }
        private void txtDocumentChg_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = Common.AllowNumeric(e);
        }
        private void txtInsurance_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = Common.AllowNumeric(e);
        }
        private void txtServiceTax_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = Common.AllowNumeric(e);
        }


        private void radBSave_Click(object sender, EventArgs e)
        {

            int intBookingIDtoValidate = 0;
            string strLRNOtoValidate = "";
            int intConsignCnttoValidate = 0;
            Boolean blnSuccess = true;
            try
            {

                if (radBSave.Text == "Update")
                {
                    TotalAmount();

                    if (!ValidateValue())
                        return;

                    DialogResult dr = MessageBox.Show("Are you sure you want to Update this luggage?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    double dblFreightChg = Convert.ToDouble((txtFreightChg.Text != "" ? txtFreightChg.Text : "0"));
                    double dblDocumentChg = Convert.ToDouble((txtDocumentChg.Text != "" ? txtDocumentChg.Text : "0"));
                    double dblServiceTax = Convert.ToDouble((txtServiceTax.Text != "" ? txtServiceTax.Text : "0"));
                    double dblInsurance = Convert.ToDouble((txtInsurance.Text != "" ? txtInsurance.Text : "0"));
                    double dblCollectionChg = Convert.ToDouble((txtCollectionChg.Text != "" ? txtCollectionChg.Text : "0"));
                    double dblDeliveryChg = Convert.ToDouble((txtDeliveryChg.Text != "" ? txtDeliveryChg.Text : "0"));
                    double dblCartage = Convert.ToDouble((txtCartage.Text != "" ? txtCartage.Text : "0"));
                    double dblHamaliChg = Convert.ToDouble((txtHamaliChg.Text != "" ? txtHamaliChg.Text : "0"));

                    if (!txtDeliveryChg.Enabled && !txtDeliveryChg.Visible)
                    {
                        dblDeliveryChg = 0;
                    }

                    double dblNetAmount = dblFreightChg + dblDeliveryChg + dblDocumentChg + dblServiceTax + dblInsurance + dblCollectionChg + dblCartage + dblHamaliChg;

                    int intModeOfPayment = Convert.ToInt32(radDModeofPayment.SelectedValue);


                    double dblDoorDelCharges = 0;


                    if (IsOrangeTypeDisplay)
                    {
                        dblDoorDelCharges = dblInsurance;
                        dblInsurance = 0;
                    }

                    string strEr = App_Code.Cargo.CargoBookingsUpdate(intShownBookingID, intCompanyID, Convert.ToInt32(radDDBookingCity.SelectedValue), Convert.ToInt32(radDDBookingBranch.SelectedValue),
                                    Convert.ToInt32(radDDDestCity.SelectedValue), Convert.ToInt32(radDDDestBranch.SelectedValue),
                                    Convert.ToInt32((chkIsPartySender.Checked ? radDPartySender.SelectedValue.ToString().Split('^')[0] : "0")), txtNameSender.Text, txtSenderAddress.Text, txtMobileNo.Text,
                                    Convert.ToInt32((chkIsPartyReceiver.Checked ? radDPartyReceiver.SelectedValue.ToString().Split('^')[0] : "0")), txtNameReceiver.Text, txtAddressReceiver.Text, txtMobileNoReceiver.Text,
                                    0, dblFreightChg, dblDocumentChg, dblServiceTax, dblInsurance, dblCollectionChg, dblDeliveryChg, dblCartage, dblNetAmount,
                                    txtComment.Text, 0, Convert.ToInt32(radDPayType.SelectedValue), intModeOfPayment, intUserID, Common.GetLogID(), dblHamaliChg, dblDoorDelCharges);


                    if (strEr != "")
                    {
                        MessageBox.Show(strEr, "Ooops..", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    else
                    {
                        MessageBox.Show("LR updated successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        RemoveValuesFromControls();
                    }
                }
                else
                {
                    TotalAmount();
                    if (!ValidateValue())
                        return;

                    if (chkCrossing.Checked && Convert.ToDecimal("0" + txtCartage.Text) < 1)
                    {
                        MessageBox.Show("CARTAGE required for CROSSING", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (radDDCStaxPaidBy.Visible && Convert.ToInt32(radDDCStaxPaidBy.SelectedValue) < 2)
                    {
                        MessageBox.Show("Please select 'Tax-Paid-By' option.");
                        return;
                    }

                    DialogResult dr = MessageBox.Show("Are you sure you want to book this luggage?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    string err = "";
                    int intBookingID = 0;
                    int items = 0;

                    if (dr == DialogResult.Yes)
                    {
                        this.Cursor = Cursors.WaitCursor;

                        DataView dv = dtGriSourse.DefaultView;

                        items = Convert.ToInt32(dtGriSourse.Compute("SUM(Qty)", dv.RowFilter));

                        double dblFreightChg = Convert.ToDouble((txtFreightChg.Text != "" ? txtFreightChg.Text : "0"));
                        double dblDocumentChg = Convert.ToDouble((txtDocumentChg.Text != "" ? txtDocumentChg.Text : "0"));
                        double dblServiceTax = Convert.ToDouble((txtServiceTax.Text != "" ? txtServiceTax.Text : "0"));
                        double dblInsurance = Convert.ToDouble((txtInsurance.Text != "" ? txtInsurance.Text : "0"));
                        double dblCollectionChg = Convert.ToDouble((txtCollectionChg.Text != "" ? txtCollectionChg.Text : "0"));
                        double dblDeliveryChg = Convert.ToDouble((txtDeliveryChg.Text != "" ? txtDeliveryChg.Text : "0"));
                        double dblCartage = Convert.ToDouble((txtCartage.Text != "" ? txtCartage.Text : "0"));
                        double dblHamaliChg = Convert.ToDouble((txtHamaliChg.Text != "" ? txtHamaliChg.Text : "0"));

                        if (!txtDeliveryChg.Enabled && !txtDeliveryChg.Visible)
                        {
                            dblDeliveryChg = 0;
                        }

                        double dblNetAmount = dblFreightChg + dblDeliveryChg + dblDocumentChg + dblServiceTax + dblInsurance + dblCollectionChg + dblCartage + dblHamaliChg;

                        int intModeOfPayment = Convert.ToInt32(radDModeofPayment.SelectedValue);

                        int intDeliveryType = 0;
                        int intCollectionType = 0;

                        if (chkIsDelivery.Checked)
                        {
                            intDeliveryType = raddDeliveryType.SelectedIndex + 1;
                        }

                        if (chkIsCollection.Checked)
                        {
                            intCollectionType = radDCollectionType.SelectedIndex + 1;
                        }

                        string partyBillNo = "";
                        if (txtBillNo.Visible && txtBillNo.Enabled)
                        {
                            DateTime dtFrom = Convert.ToDateTime(dtBillNo.Text);
                            partyBillNo = txtBillNo.Text + "~" + dtFrom.ToString("dd-MMM-yyyy");
                        }

                        string EwayBillNo = "";
                        if (txtEwayBillNo.Visible && txtEwayBillNo.Enabled)
                        {
                            //DateTime dtFrom = Convert.ToDateTime(dtEwayBillno.Text);
                            EwayBillNo = txtEwayBillNo.Text + "~" + dtEwayBillStartDate.ToString("dd-MMM-yyyy") + "~" + dtEwayBillEndDate.ToString("dd-MMM-yyyy");
                        }

                        //double dblHamaliCharges = 0;
                        double dblDoorDelCharges = 0;
                        if (IsOrangeTypeDisplay)
                        {
                            //dblHamaliCharges = dblServiceTax;
                            //dblServiceTax = 0;

                            dblNetAmount = dblNetAmount - dblHamaliChg;

                            dblDoorDelCharges = dblInsurance;
                            dblInsurance = 0;
                        }

                        string strManualLR = "";

                        if (txtManualLR.Visible)
                            strManualLR = txtManualLR.Text;

                        int errDtCnt = 0;

                        string errMsgs = "";

                        //string strBookingDetailsData = "";
                        string strAllConsignmentSubTypeID = "";
                        string strAllDescription = "";
                        string strAllQuantity = "";
                        string strAllGoodsValue = "";
                        string strAllVolume = "";
                        string strAllActualWeight = "";
                        string strAllChargedWeight = "";
                        string strAllActualRate = "";
                        string strAllRate = "";
                        string strAllFreight = "";
                        string strAllAmount = "";
                        string strAllCurrentCity = "";
                        string strAllCurrentBranch = "";

                        foreach (DataRow drw in dtGriSourse.Rows)
                        {
                            //string errMsg = "";
                            //int intBookingDetID = 0;
                            intBookingID = 0;
                            int ConsignmentSubTypeID = Convert.ToInt32(drw["ConsignmentTypeID"].ToString().Split('-')[0]);
                            string strDescription = drw["Description"].ToString();
                            int intQuantity = Convert.ToInt32(drw["Qty"]);
                            double dblGoodsValue;
                            double dblActualWeight = 0;
                            try
                            {
                                dblGoodsValue = Convert.ToDouble((drw["Goodsvalue"].ToString() == "" ? 0 : drw["Goodsvalue"]));
                                dblActualWeight = Convert.ToDouble((drw["Weight"].ToString() == "" ? 0 : drw["Weight"])); //Convert.ToDouble(drw["Weight"]);
                            }
                            catch (Exception ex)
                            {
                                dblGoodsValue = 0;
                            }
                            string strVolume = "0";

                            double dblChargedWeight = 0;
                            double dblActualRate = Convert.ToDouble(drw["Rate"]);
                            double dblRate = Convert.ToDouble(drw["Rate"]);
                            double dblFreight = Convert.ToDouble(drw["Freight"]);
                            double dblAmount = Convert.ToDouble(drw["Freight"]);
                            int intCurrentCity = Common.GetBranchCityID();
                            int intCurrentBranch = Common.GetBranchID();

                            if ((ConsignmentSubTypeID != 0 && intQuantity != 0 && (dblRate != 0 || is_direct_freight_company) && dblFreight != 0) || radDPayType.SelectedValue.ToString() == "3")
                            {
                                intConsignCnttoValidate++;

                                strAllConsignmentSubTypeID += "||" + ConsignmentSubTypeID;
                                strAllDescription += "||" + strDescription;
                                strAllQuantity += "||" + intQuantity;
                                strAllGoodsValue += "||" + dblGoodsValue;
                                strAllVolume += "||" + strVolume;
                                strAllActualWeight += "||" + dblActualWeight;
                                strAllChargedWeight += "||" + dblChargedWeight;
                                strAllActualRate += "||" + dblActualRate;
                                strAllRate += "||" + dblRate;
                                strAllFreight += "||" + dblFreight;
                                strAllAmount += "||" + dblAmount;
                                strAllCurrentCity += "||" + intCurrentCity;
                                strAllCurrentBranch += "||" + intCurrentBranch;

                                //strBookingDetailsData += "~~" + ConsignmentSubTypeID + "||" + strDescription + "||" + intQuantity + "||" +
                                //                        dblGoodsValue + "||" + strVolume + "||" + dblActualWeight + "||" + dblChargedWeight + "||" + dblActualRate + "||" +
                                //                        dblRate + "||" + dblFreight + "||" + dblAmount + "||" + intCurrentCity + "||" + intCurrentBranch;

                            }
                        }

                        if (intConsignCnttoValidate > 0)
                        {
                            strAllConsignmentSubTypeID = strAllConsignmentSubTypeID.Substring(2);
                            strAllDescription = strAllDescription.Substring(2);
                            strAllQuantity = strAllQuantity.Substring(2);
                            strAllGoodsValue = strAllGoodsValue.Substring(2);
                            strAllVolume = strAllVolume.Substring(2);
                            strAllActualWeight = strAllActualWeight.Substring(2);
                            strAllChargedWeight = strAllChargedWeight.Substring(2);
                            strAllActualRate = strAllActualRate.Substring(2);
                            strAllRate = strAllRate.Substring(2);
                            strAllFreight = strAllFreight.Substring(2);
                            strAllAmount = strAllAmount.Substring(2);
                            strAllCurrentCity = strAllCurrentCity.Substring(2);
                            strAllCurrentBranch = strAllCurrentBranch.Substring(2);
                        }

                        int intCrossingCity, intCrossingBranch;
                        intCrossingBranch = intCrossingCity = 0;
                        if (chkCrossing.Checked)
                        {
                            intCrossingCity = Convert.ToInt32(radDDCrossingCity.SelectedValue);
                            intCrossingBranch = Convert.ToInt32(radDDCrossingBranch.SelectedValue);
                        }

                        string staxPaidBy = "";

                        if (radDDCStaxPaidBy.Visible && (radDDCStaxPaidBy.SelectedValue.ToString() != null && radDDCStaxPaidBy.SelectedValue.ToString() != "1"))
                        {
                            if (radDDCStaxPaidBy.Text.ToLower().Trim() == "transporter")
                            {
                                staxPaidBy = "Tr";
                            }
                            else if (radDDCStaxPaidBy.Text.ToLower().Trim() == "consignor")
                            {
                                staxPaidBy = "Cr";
                            }
                            else if (radDDCStaxPaidBy.Text.ToLower().Trim() == "consignee")
                            {
                                staxPaidBy = "Cn";
                            }
                        }

                        string LrExists = App_Code.Cargo.CargoBookingExists(intCompanyID, Common.GetBranchCityID(), Common.GetBranchID(), Convert.ToInt32(radDDDestCity.SelectedValue), Convert.ToInt32(radDDDestBranch.SelectedValue), txtMobileNo.Text, txtMobileNoReceiver.Text, intUserID);
                        if (LrExists != "")
                        {
                            DialogResult ContinueBooking = MessageBox.Show(LrExists + "\n" + "Would you still like to book this consignment?", "Booking Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                            if (ContinueBooking == System.Windows.Forms.DialogResult.No)
                            {
                                return;
                            }
                        }

                        string strsenderimgurl = "";
                        string strIDProofimgurl = "";

                        if (strSenderImageName != "")
                        {
                            strsenderimgurl = "https://crs-cargo-proof.s3.amazonaws.com/" + strSenderImageName;
                        }

                        if (strIDProofImageName != "")
                        {
                            strIDProofimgurl = "https://crs-cargo-proof.s3.amazonaws.com/" + strIDProofImageName;
                        }

                        DataTable dt = App_Code.Cargo.CargoBookingsInsertUpdateV2(
                                        intBookingID, intCompanyID, Common.GetBranchCityID(),
                                        Common.GetBranchID(),
                                        Convert.ToInt32(radDDDestCity.SelectedValue), Convert.ToInt32(radDDDestBranch.SelectedValue),
                                        Convert.ToInt32(radDPayType.SelectedValue),
                                        (chkIsPartySender.Checked ? 1 : 0), (chkIsPartyReceiver.Checked ? 1 : 0),
                                        Convert.ToInt32((chkIsPartySender.Checked ? radDPartySender.SelectedValue.ToString().Split('^')[0] : "0")),
                                        txtNameSender.Text,
                                        txtSenderAddress.Text, txtMobileNo.Text, partyBillNo,
                                        Convert.ToInt32((chkIsPartyReceiver.Checked ? radDPartyReceiver.SelectedValue.ToString().Split('^')[0] : "0")),
                                        txtNameReceiver.Text, txtAddressReceiver.Text,
                                        txtMobileNoReceiver.Text, items, dblFreightChg,
                                        dblDocumentChg, dblServiceTax, dblInsurance,
                                        dblCollectionChg, dblCartage, dblNetAmount,
                                        txtComment.Text, intUserID,
                                        intModeOfPayment, Common.GetCacheGUID(),
                                        dblDeliveryChg, intDeliveryType, intCollectionType,
                                        dblHamaliChg, dblDoorDelCharges,
                                        strManualLR, intCrossingCity, intCrossingBranch, Common.GetLogID(), staxPaidBy,
                                        txtSenderGSTN.Text, txtReceiverGSTN.Text,
                                        intIDProofTypeID, strIDProofNo, strsenderimgurl, strIDProofimgurl,
                                        Common.GetBranchCityID(), Common.GetBranchID(),
                                        strAllConsignmentSubTypeID, strAllDescription, strAllQuantity, strAllGoodsValue, strAllVolume,
                                        strAllActualWeight, strAllChargedWeight, strAllActualRate, strAllRate, strAllFreight, strAllAmount,
                                        strAllCurrentCity, strAllCurrentBranch, EwayBillNo, ref err);

                        if (err == "" && dt != null && dt.Rows.Count > 0)
                        {
                            try
                            {
                                intBookingIDtoValidate = Convert.ToInt32(dt.Rows[0]["BookingID"].ToString());
                                strLRNOtoValidate = dt.Rows[0]["LRNo"].ToString();
                                //intConsignCnttoValidate = 0;
                            }
                            catch (Exception exm)
                            { }
                            if (errMsgs == "" && errDtCnt == 0)
                            {
                                if (intBookingIDtoValidate == 0)
                                    intBookingIDtoValidate = Convert.ToInt32(dt.Rows[0]["BookingID"].ToString());

                                blnSuccess = true;
                                ValidateBooking(intBookingIDtoValidate, intCompanyID, intBranchID, intUserID, Common.GetLogID(), strLRNOtoValidate, intConsignCnttoValidate, ref blnSuccess);

                                if (!blnSuccess)
                                    return;

                                intBookingIDtoValidate = 0;
                                strLRNOtoValidate = "";
                                chkCrossing.Checked = false;
                                //radDDCStaxPaidBy.SelectedIndex = 0;
                                if (is_stax_company)
                                {
                                    radDDCStaxPaidBy.Visible = true;
                                    radDDCStaxPaidBy.Enabled = false;
                                    radDDCStaxPaidBy.SelectedValue = 2;

                                }
                                else
                                {
                                    radDDCStaxPaidBy.Visible = false;
                                    radDDCStaxPaidBy.Enabled = false;
                                    radDDCStaxPaidBy.SelectedValue = 1;
                                }
                                MessageBox.Show("Luggage successfully booked.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);


                                RemoveValuesFromControls();

                                lblLRNo.Visible = true;
                                lblTotalAmtCnf.Visible = true;

                                lblLRNo.Text = "LRN : " + dt.Rows[0]["LRNo"].ToString() + "  Sender : " + dt.Rows[0]["Sender"].ToString()
                                                    + "  Receiver : " + dt.Rows[0]["RecName"].ToString()
                                                    + (txtFreightChg.Visible == true ? "  Freight : " + dt.Rows[0]["Freight"].ToString() : "")
                                                    + (txtDeliveryChg.Visible == true ? "  Delivery chg : " + dt.Rows[0]["DeliveryCharges"].ToString() : "")
                                                    + (txtCollectionChg.Visible == true ? "  Collection chg : " + dt.Rows[0]["CollectionCharges"].ToString() : "")
                                                    + (txtCartage.Visible == true ? "  Cartage : " + dt.Rows[0]["CartageAmount"].ToString() : "")
                                                    + (txtDocumentChg.Visible == true ? "  Document : " + dt.Rows[0]["DocumentCharges"].ToString() : "")


                                                    + (lblInsuranceBx.Text == "Door-Del.Chg" ? "  Door-Del.Chg : " + dt.Rows[0]["DoorDelCharges"].ToString() : "  Insurance : " + dt.Rows[0]["Insurance"].ToString())
                                    //+ (txtInsurance.Visible == true ? "  Insurance : " + dt.Rows[0]["Insurance"].ToString() : "")

                                                    + (lblServiceTxBx.Text == "Hamali Chg" ? "  Hamali Chg : " + dt.Rows[0]["HamaliCharges"].ToString() : "  Service tax : " + (Convert.ToDouble("0" + dt.Rows[0]["ServiceTaxAmount"].ToString()) + Convert.ToDouble("0" + dt.Rows[0]["ServiceTaxAmountCartage"].ToString())).ToString());
                                //+ (txtServiceTax.Visible == true ? "  Service tax : " + dt.Rows[0]["ServiceTaxAmount"].ToString() : "")                                                            

                                lblTotalAmtCnf.Text = "Total : " + dt.Rows[0]["NetAmount"].ToString();

                                dlgUploadImgtos3 TG = new dlgUploadImgtos3(UploadImgtoS3);
                                TG.BeginInvoke(null, null);

                                DialogResult dr1 = MessageBox.Show("Do you want to take a print?", "Print", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                                if (dr1 == DialogResult.Yes)
                                {
                                    int _intBookingID = Convert.ToInt32(dt.Rows[0]["BookingID"]);
                                    string _StrLRNO = dt.Rows[0]["LRNo"].ToString();
                                    DataTable dtBooking = App_Code.Cargo.CargoBookingDetailsToPrint(_intBookingID, intBranchID, intUserID, _StrLRNO);
                                    //string barcode_image_path= Common.GetBarCodeImage(_intBookingID.ToString());

                                    if (dtBooking != null && dtBooking.Rows.Count > 0)
                                    {
                                        Cargo.frmPrintTicket frmPrint = new Cargo.frmPrintTicket(Convert.ToInt32(dt.Rows[0]["BookingID"]), intBranchID, intUserID,
                                                                    dt.Rows[0]["LRNo"].ToString(), dtBooking, false, strLaserPrinterName, strStickerPrinterName);
                                        frmPrint.ShowDialog();
                                    }
                                    else
                                    {
                                        MessageBox.Show("No data found to print.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }

                                }
                                //lblSenderPartyCnf.Visible = true;
                                //lblReceiverPartyCnf.Visible = true;
                                //lblAmtCnf.Visible = true;
                                //lblRemarksCnf.Visible = true;


                                //lblSenderPartyCnf.Text = "Sender : " + dt.Rows[0]["Sender"].ToString();
                                //lblReceiverPartyCnf.Text = "Receiver : " + dt.Rows[0]["RecName"].ToString();
                                //lblAmtCnf.Text = "Total : " + dt.Rows[0]["NetAmount"].ToString();
                                //lblRemarksCnf.Text = "Commeng : " + dt.Rows[0]["Remarks"].ToString();

                                radDDDestCity.Focus();
                            }
                        }
                        else
                        {
                            MessageBox.Show(err, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (radBSave.Text != "Update")
                {
                    if (blnSuccess)
                    {
                        if (intBookingIDtoValidate != 0 || strLRNOtoValidate != "")
                            ValidateBooking(intBookingIDtoValidate, intCompanyID, intBranchID, intUserID, Common.GetLogID(), strLRNOtoValidate, intConsignCnttoValidate, ref blnSuccess);
                    }
                }
                this.Cursor = Cursors.Default;
            }
        }

        private void UploadImgtoS3()
        {
            try
            {
                AmazonUploader amz = new AmazonUploader();
                string path = "C:\\Windows\\Temp\\DosPrint\\";

                if (strSenderImageName != "")
                {
                    amz.UploadFile(path + strSenderImageName, strSenderImageName, "crs-cargo-proof");
                }

                if (strIDProofImageName != "")
                {
                    amz.UploadFile(path + strIDProofImageName, strIDProofImageName, "crs-cargo-proof");
                }

                strSenderImageName = "";
                strIDProofImageName = "";
            }
            catch (Exception)
            {

            }
        }

        public void ValidateBooking(int intBookingID, int intCompanyID, int intBranchID, int intUserID, int intLogID, string strLRNo, int intConsignCount, ref Boolean blnSuccess)
        {
            try
            {
                string errMsg = App_Code.Cargo.CargoBookingsValidate(intBookingID, intCompanyID, intBranchID, intUserID, intLogID, strLRNo, intConsignCount);

                if (errMsg.Replace(":", "") != "")
                {
                    blnSuccess = false;
                    MessageBox.Show(errMsg.Replace(":", ""), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    string err = App_Code.Cargo.CargoSendSMS(intBookingID, intCompanyID, intUserID, intLogID, "B");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public Boolean ValidateValue()
        {
            if (radDDBookingCity.Items.Count == 0)
            {
                MessageBox.Show("Please select Booking City.");
                radDDBookingCity.Focus();
                return false;
            }

            if (Convert.ToInt32(radDDBookingCity.SelectedValue) != intBranchCityID)
            {
                MessageBox.Show("Booking City is not your city.");
                return false;
            }

            if (radDDBookingBranch.Items.Count == 0)
            {
                MessageBox.Show("Please select Booking Branch.");
                radDDBookingBranch.Focus();
                return false;
            }

            if (Convert.ToInt32(radDDBookingBranch.SelectedValue) != intBranchID)
            {
                MessageBox.Show("Booking Branch is not your Branch.");
                return false;
            }

            if (radDDDestCity.Items.Count == 0)
            {
                MessageBox.Show("Please select Destination City.");
                radDDDestCity.Focus();
                return false;
            }

            if (radDDDestBranch.Items.Count == 0)
            {
                MessageBox.Show("Please select Destination Branch.");
                radDDDestBranch.Focus();
                return false;
            }

            if (radDPayType.Items.Count == 0)
            {
                MessageBox.Show("Please select Pay Type.");
                radDPayType.Focus();
                return false;
            }

            if (txtNameSender.Text == "")
            {
                MessageBox.Show("Please enter sender name.");
                txtNameSender.Focus();
                return false;
            }

            if (txtNameReceiver.Text == "")
            {
                MessageBox.Show("Please enter Receiver Name.");
                txtNameReceiver.Focus();
                return false;
            }


            //if (txtMobileNo.TextLength != 10)
            //{
            //    MessageBox.Show("Please enter proper sender Mobile No. It should be 10 digits.");
            //    txtMobileNo.Focus();
            //    return false;
            //}


            if (txtMobileNoReceiver.Text == "")
            {
                txtMobileNoReceiver.Text = "0";
                //MessageBox.Show("Please enter Receiver Mobile No.");
                //txtMobileNoReceiver.Focus();
                //return false;
            }

            if (txtMobileNo.Text == "")
            {
                txtMobileNo.Text = "0";
                //MessageBox.Show("Please enter Sender Mobile No.");
                //txtMobileNo.Focus();
                //return false;
            }

            //if (txtMobileNoReceiver.TextLength != 10)
            //{
            //    MessageBox.Show("Please enter proper receiver Mobile No. It should be 10 digits.");
            //    txtMobileNoReceiver.Focus();
            //    return false;
            //}

            decimal total_freight = 0, dblGoodsValue = 0;
            for (int i = 0; i < radGridConsignItems.Rows.Count; i++)
            {
                try
                {
                    string ConsignmentSubType = radGridConsignItems.Rows[i].Cells["ConsignmentType"].Value.ToString();
                    string Description = radGridConsignItems.Rows[i].Cells["Description"].Value.ToString();
                    string Qty = radGridConsignItems.Rows[i].Cells["Qty"].Value.ToString();
                    string Goodsvalue = radGridConsignItems.Rows[i].Cells["Goodsvalue"].Value.ToString();
                    string Rate = radGridConsignItems.Rows[i].Cells["Rate"].Value.ToString();
                    dblGoodsValue += Convert.ToDecimal(Goodsvalue);

                    try
                    {
                        total_freight += Convert.ToDecimal("0" + radGridConsignItems.Rows[i].Cells["Freight"].Value.ToString());
                    }
                    catch (Exception ex)
                    {
                    }

                    if (ConsignmentSubType != "0" || Description != "" || (Qty != "" && Qty != "0") || (Goodsvalue != "" && Goodsvalue != "0") || (Rate != "" && Rate != "0") || radGridConsignItems.Rows.Count == 1)
                    {
                        if (ConsignmentSubType == "0")
                        {
                            MessageBox.Show("Please select ConsignmentType");
                            radGridConsignItems.Rows[i].Cells["ConsignmentType"].BeginEdit();
                            return false;
                        }
                        /*if (Description == "")
                        {
                            MessageBox.Show("Please Enter Description.");
                            radGridConsignItems.Rows[i].Cells["Description"].BeginEdit();
                            return false;
                        }*/
                        if (Qty == "0" || Qty == "")
                        {
                            MessageBox.Show("Please Enter Unit.");
                            radGridConsignItems.Rows[i].Cells["Qty"].BeginEdit();
                            return false;
                        }
                        /*if (Goodsvalue == "0" || Goodsvalue == "")
                        {
                            MessageBox.Show("Please Enter Goodsvalue.");
                            radGridConsignItems.Rows[i].Cells["Goodsvalue"].BeginEdit();
                            return false;
                        }*/

                        if ((Rate == "0" || Rate == "") && radDPayType.SelectedValue.ToString() != "3" && !is_direct_freight_company)
                        {
                            MessageBox.Show("Please Enter Rate.");
                            radGridConsignItems.Rows[i].Cells["Rate"].BeginEdit();
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            if (intIsEwayBillNo == 1 && txtEwayBillNo.Visible)
            {
                if ((txtEwayBillNo.Text.Trim() == "") && dblGoodsValue >= 50000)
                {
                    MessageBox.Show("E-WayBill No. needs to be entered for Goods Value greater than equal to Rs. 50,000/-");
                    txtEwayBillNo.Focus();
                    return false;
                }
            }

            if (intCompanyID == 168)
            {
                if (total_freight > dcmMinTaxAmount) //750
                {
                    MessageBox.Show("Freight Amount should not be greater than " + dcmMinTaxAmount + "!", "Wrong", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            if (txtAmount.Text == "0" && radDPayType.SelectedValue.ToString() != "3")
            {
                MessageBox.Show("Booking can not be done for 0 Amount.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (txtDeliveryChg.Enabled && txtDeliveryChg.Visible)
                    txtDeliveryChg.Focus();
                else if (txtCollectionChg.Enabled && txtCollectionChg.Visible)
                    txtCollectionChg.Focus();
                else if (txtCartage.Enabled && txtCartage.Visible)
                    txtCartage.Focus();
                else if (txtDocumentChg.Enabled && txtDocumentChg.Visible)
                    txtDocumentChg.Focus();
                else if (txtInsurance.Enabled && txtInsurance.Visible)
                    txtInsurance.Focus();
                else if (txtServiceTax.Enabled && txtServiceTax.Visible)
                    txtServiceTax.Focus();
                else
                    txtComment.Focus();

                return false;
            }

            else if (radDPayType.SelectedValue.ToString() == "3" && Convert.ToInt32(txtAmount.Text) > 0)
            {
                //Changed By Prateek: 3rd condition added as hamali-charge can be collected for FOC bookings for Shatabdi. 03-feb-2014, chargo changes email from Krishna Singh
                if (intCompanyID == 1247 || intCompanyID == 1 || intCompanyID == 1008 || intCompanyID == 977 || intCompanyID == 225 || intCompanyID == 1427 || intCompanyID == 422)
                {
                    if ((Convert.ToInt32(txtAmount.Text) - Convert.ToInt32(txtHamaliChg.Text)) != 0)
                    {
                        MessageBox.Show("Booking can be done only with 0 Amount for FOC Type.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
                else
                {
                    MessageBox.Show("Booking can be done only with 0 Amount for FOC Type.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            if (txtServiceTax.Text.ToString() != "" && Convert.ToDouble(txtServiceTax.Text) > 0)
            {
                if (Convert.ToInt32(radDPayType.SelectedValue) == 2)
                {
                    if (txtReceiverGSTN.Text.Trim() != "")
                    {
                        MessageBox.Show("ServiceTax should not applied if Receiver GSTN is given.");
                        return false;
                    }
                }
                else
                {
                    if (txtSenderGSTN.Text.Trim() != "")
                    {
                        MessageBox.Show("ServiceTax should not applied if Sender GSTN is given.");
                        return false;
                    }
                }
            }

            return true;
        }

        public void RemoveValuesFromControls()
        {
            try
            {
                chkCrossing.Checked = false;
                radDDBookingCity.Enabled = false;
                radDDBookingBranch.Enabled = false;

                radDDBookingCity.SelectedValue = intBranchCityID.ToString();
                radDDBookingBranch.SelectedValue = intBranchID.ToString();

                intShownBookingID = 0;
                radDPayType.SelectedIndex = 0;
                chkIsPartySender.Checked = false;
                chkIsPartyReceiver.Checked = false;
                txtNameSender.Text = "";
                txtNameReceiver.Text = "";
                txtMobileNo.Text = "";
                txtMobileNoReceiver.Text = "";
                txtAddressReceiver.Text = "";

                //chkRecMobileGetData.Enabled = false;
                //chkSenderMobileGetData.Enabled = false;
                chkRecMobileGetData.Checked = false;
                chkSenderMobileGetData.Checked = false;

                txtSenderAddress.Text = "";


                for (int i = dtGriSourse.Rows.Count - 1; i >= 0; i--)
                {
                    dtGriSourse.Rows.RemoveAt(i);
                }
                //foreach (DataRow drw in dtGriSourse.Rows)
                //{
                //    dtGriSourse.Rows.Remove(drw);
                //}

                AddNewRow("0", "", "0", "0", "0", "0", 0);

                radGridConsignItems.DataSource = dtGriSourse;
                radGridConsignItems.Enabled = true;

                txtFreightChg.Text = "0";
                txtCollectionChg.Text = "0";
                txtDeliveryChg.Text = "0";
                txtCartage.Text = "0";
                txtDocumentChg.Text = "0";
                txtInsurance.Text = "0";
                txtHamaliChg.Text = "0";
                txtServiceTax.Text = "0";
                txtTotal.Text = "0";
                txtAmount.Text = "0";
                txtComment.Text = "";
                lblLRNo.Text = "";
                lblLRNo.Visible = false;
                lblTotalAmtCnf.Visible = false;
                lblTotalAmtCnf.Text = "";


                radDDDestCity.Enabled = true;
                radDDDestBranch.Enabled = true;
                radDPayType.Enabled = true;
                chkIsPartySender.Enabled = true;

                txtNameSender.ReadOnly = false;
                txtMobileNo.ReadOnly = false;
                txtNameReceiver.ReadOnly = false;
                txtMobileNoReceiver.ReadOnly = false;
                txtAddressReceiver.ReadOnly = false;
                txtCollectionChg.ReadOnly = false;
                txtDeliveryChg.ReadOnly = false;
                txtCartage.ReadOnly = false;
                txtDocumentChg.ReadOnly = false;
                txtInsurance.ReadOnly = false;
                txtHamaliChg.ReadOnly = false;
                //txtServiceTax.ReadOnly = false;
                txtComment.ReadOnly = false;

                chkIsPartyReceiver.Enabled = true;
                radDModeofPayment.Enabled = false;
                radGridConsignItems.Enabled = true;
                radBSave.Enabled = true;
                radBSave.Text = "Save";

                txtBillNo.Text = "";
                txtBillNo.ReadOnly = false;
                dtBillNo.Enabled = true;
                ////radDDDDBill.Enabled = true;
                ////radDDMMBill.Enabled = true;
                ////radDDYYBill.Enabled = true;

                txtEwayBillNo.Text = "";
                txtEwayBillNo.ReadOnly = false;
               

                fillDates();

                txtSenderGSTN.Text = "";
                txtReceiverGSTN.Text = "";

                intIDProofTypeID = 0;
                strIDProofNo = "";
                //strSenderImageName = "";
                //strIDProofImageName = "";

                lnkimageandID.Text = "Capture Image and ID";
                ResetEwayBillDates();
            }
            catch (Exception ex)
            { }
        }

        private void radDDDestCity_Enter(object sender, EventArgs e)
        {
            ((Telerik.WinControls.UI.RadDropDownList)sender).DropDownListElement.EditableElement.BackColor = objCurrColor;
        }

        private void radDDDestCity_Leave(object sender, EventArgs e)
        {
            ((Telerik.WinControls.UI.RadDropDownList)sender).DropDownListElement.EditableElement.BackColor = Color.White;
        }

        private void radBAddNew_Click(object sender, EventArgs e)
        {
            RemoveValuesFromControls();
            radDDDestCity.Focus();
        }

        private void radBCancel_Click(object sender, EventArgs e)
        {
            try
            {
                if (Application.OpenForms["frmLuggageBooking"] != null)
                    Application.OpenForms["frmLuggageBooking"].Close();
                if (Application.OpenForms["frmBookingV3"] != null)
                    Application.OpenForms["frmBookingV3"].Close();

                if (Application.OpenForms["frmBookingOffline"] != null)
                    Application.OpenForms["frmBookingOffline"].Close();
                if (Application.OpenForms["frmSplashScreen"] != null)
                    Application.OpenForms["frmSplashScreen"].Close();
                ((CRSMDIParent)Application.OpenForms["CRSMDIParent"]).Close();
            }
            catch (Exception ex)
            { }
            //Application.OpenForms["frmSplashScreen"].Close();
            //this.Close();
        }

        private void txtCollectionChg_Leave(object sender, EventArgs e)
        {
            TotalAmount();
        }

        public void TotalAmount()
        {
            double dblFreightChg = Convert.ToDouble((txtFreightChg.Text != "" ? txtFreightChg.Text : "0"));
            double dblDocumentChg = Convert.ToDouble((txtDocumentChg.Text != "" ? txtDocumentChg.Text : "0"));
            double dblServiceTax = Convert.ToDouble((txtServiceTax.Text != "" ? txtServiceTax.Text : "0"));
            double dblInsurance = Convert.ToDouble((txtInsurance.Text != "" ? txtInsurance.Text : "0"));
            double dblDeliveryChg = Convert.ToDouble((txtDeliveryChg.Text != "" ? txtDeliveryChg.Text : "0"));
            double dblCollectionChg = Convert.ToDouble((txtCollectionChg.Text != "" ? txtCollectionChg.Text : "0"));
            double dblCartage = Convert.ToDouble((txtCartage.Text != "" ? txtCartage.Text : "0"));
            double dblHamaliChg = Convert.ToDouble((txtHamaliChg.Text != "" ? txtHamaliChg.Text : "0"));
            //if (is_stax_company)
            //{
            //    txtTotal.Text = (dblDeliveryChg + dblFreightChg + dblDocumentChg + dblServiceTax + dblInsurance + dblCollectionChg + dblCartage + dblHamaliChg).ToString();
            //}
            //else
            //{
            //    txtTotal.Text = (dblDeliveryChg + dblFreightChg + dblDocumentChg + dblInsurance + dblCollectionChg + dblCartage + dblHamaliChg).ToString();
            //}
            //if (blnIsShow)
            //    return;

            double dblTotalAmt = 0;
            //if (intCompanyID != 99 && intCompanyID != 1)
            if (intCompanyID != 0)
            {
                if (is_stax_company)
                {
                    int intServiceTaxPayer = 1;

                    if (Convert.ToInt32(radDPayType.SelectedValue) == 1 && radDPartySender.SelectedIndex > -1)
                    {
                        string[] strPartyDetails = radDPartySender.SelectedValue.ToString().Split('^');
                        intServiceTaxPayer = Convert.ToInt32(strPartyDetails[3]);
                    }
                    else if (Convert.ToInt32(radDPayType.SelectedValue) == 2 && radDPartyReceiver.SelectedIndex > -1)
                    {
                        string[] strPartyDetails = radDPartyReceiver.SelectedValue.ToString().Split('^');
                        intServiceTaxPayer = Convert.ToInt32(strPartyDetails[3]);
                    }

                    if (intServiceTaxPayer == 2 || intServiceTaxPayer == 3 || dblFreightChg > Convert.ToDouble(dcmMinTaxAmount))
                    {
                        dblServiceTax = 0;
                        txtServiceTax.Text = dblServiceTax.ToString();
                    }


                    if (dblFreightChg + dblCartage >= Convert.ToDouble(dcmMinTaxAmount))
                    {
                        dblServiceTax = 0;
                        if (is_stax_company && radDDCStaxPaidBy.Visible && radDDCStaxPaidBy.SelectedValue.ToString() == "2")
                        {
                            dblServiceTax = (dblFreightChg + dblCartage) * Convert.ToDouble(dcmTaxPCT) / 100.00;
                        }
                        txtServiceTax.Text = dblServiceTax.ToString();
                    }
                }

                dblTotalAmt = dblDeliveryChg + dblFreightChg + dblDocumentChg + dblServiceTax + dblInsurance + dblCollectionChg + dblCartage + dblHamaliChg;
                txtAmount.Text = dblTotalAmt.ToString();
            }
            else
            {
                // KTN : Not Going Here, Need to check why this has been written
                dblTotalAmt = dblDeliveryChg + dblFreightChg + dblDocumentChg + dblInsurance + dblCollectionChg + dblCartage + dblHamaliChg;

                if (dblTotalAmt >= 750)
                {
                    dblServiceTax = (dblTotalAmt * 12.36) / 100;
                    dblTotalAmt = dblTotalAmt + dblServiceTax;

                    txtServiceTax.Text = dblServiceTax.ToString();
                }
                txtAmount.Text = dblTotalAmt.ToString();
            }
        }
        private void radGridConsignItems_CellBeginEdit(object sender, GridViewCellCancelEventArgs e)
        {
            //MessageBox.Show("hi");
        }

        //private void radGridConsignItems_CreateCell(object sender, GridViewCreateCellEventArgs e)
        //{
        //    if(e.CellElement != null)
        //        SubscribeEvents(e.CellElement);
        //}

        //private void SubscribeEvents(GridCellElement control)
        //{
        //    control.KeyPress += new KeyPressEventHandler(radGridConsignItems_KeyPress);
        //    //control.ControlAdded += new ControlEventHandler(control_ControlAdded);
        //    //control.ControlRemoved += new ControlEventHandler(control_ControlRemoved);

        //    //foreach (Control innerControl in control.Controls)
        //    //{
        //    //    SubscribeEvents(innerControl);
        //    //}
        //}

        public void setPaymentMode()
        {
            try
            {
                radDModeofPayment.SelectedIndex = 0;

                if (radDPayType.SelectedValue.ToString() == "1")
                {
                    if (!chkIsPartySender.Checked && !chkIsPartyReceiver.Checked)
                        radDModeofPayment.Enabled = false;
                    else if (chkIsPartySender.Checked && !chkIsPartyReceiver.Checked)
                        radDModeofPayment.Enabled = true;
                    else if (!chkIsPartySender.Checked && chkIsPartyReceiver.Checked)
                        radDModeofPayment.Enabled = false;
                    else if (chkIsPartySender.Checked && chkIsPartyReceiver.Checked)
                        radDModeofPayment.Enabled = true;
                }
                else if (radDPayType.SelectedValue.ToString() == "2")
                {
                    if (!chkIsPartySender.Checked && !chkIsPartyReceiver.Checked)
                        radDModeofPayment.Enabled = false;
                    else if (chkIsPartySender.Checked && !chkIsPartyReceiver.Checked)
                        radDModeofPayment.Enabled = false;
                    else if (!chkIsPartySender.Checked && chkIsPartyReceiver.Checked)
                        radDModeofPayment.Enabled = true;
                    else if (chkIsPartySender.Checked && chkIsPartyReceiver.Checked)
                        radDModeofPayment.Enabled = true;
                }
                else if (radDPayType.SelectedValue.ToString() == "3")
                {
                    if (!chkIsPartySender.Checked && !chkIsPartyReceiver.Checked)
                        radDModeofPayment.Enabled = false;
                    else if (chkIsPartySender.Checked && !chkIsPartyReceiver.Checked)
                        radDModeofPayment.Enabled = false;
                    else if (!chkIsPartySender.Checked && chkIsPartyReceiver.Checked)
                        radDModeofPayment.Enabled = false;
                    else if (chkIsPartySender.Checked && chkIsPartyReceiver.Checked)
                        radDModeofPayment.Enabled = false;
                }
                else if (radDPayType.SelectedValue.ToString() == "4")
                {
                    if (!chkIsPartySender.Checked && !chkIsPartyReceiver.Checked)
                        radDModeofPayment.Enabled = false;
                    else if (chkIsPartySender.Checked && !chkIsPartyReceiver.Checked)
                        radDModeofPayment.Enabled = false;
                    else if (!chkIsPartySender.Checked && chkIsPartyReceiver.Checked)
                        radDModeofPayment.Enabled = true;
                    else if (chkIsPartySender.Checked && chkIsPartyReceiver.Checked)
                        radDModeofPayment.Enabled = true;
                }
                else if (radDPayType.SelectedValue.ToString() == "5")
                {
                    radDModeofPayment.SelectedValue = "2";
                    radDModeofPayment.Enabled = false;
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void radButton10_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            tableLayoutPanel1.Enabled = false;
            tableLayoutPanel1.BackColor = Color.Silver;
            //frmAdmin objFrmAdmin = (frmAdmin)Application.OpenForms["frmAdmin"];
            Cargo.frmDispatch frm = new Cargo.frmDispatch();
            //frm.Width = this.Width - 200;
            //frm.Height = this.Height - 200;
            frm.ShowDialog();
            this.Cursor = Cursors.Default;
            //objFrmAdmin.SetForm(frm);
            tableLayoutPanel1.BackColor = Color.White;
            tableLayoutPanel1.Enabled = true;
        }

        private void radButton11_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            tableLayoutPanel1.Enabled = false;
            tableLayoutPanel1.BackColor = Color.Silver;
            //frmAdmin objFrmAdmin = (frmAdmin)Application.OpenForms["frmAdmin"];
            Cargo.frmReceipt frm = new Cargo.frmReceipt();
            //frm.Width = this.Width - 200;
            //frm.Height = this.Height - 200;
            frm.ShowDialog();
            this.Cursor = Cursors.Default;
            //objFrmAdmin.SetForm(frm);
            tableLayoutPanel1.BackColor = Color.White;
            tableLayoutPanel1.Enabled = true;
        }

        private void radBDelivery_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                if (txtLRNo.Text == "")
                {
                    MessageBox.Show("Please enter valid LRNo.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtLRNo.Focus();
                    return;
                }

                DataSet ds = new DataSet();

                ds = App_Code.Cargo.GetLuggageForDelivery(txtLRNo.Text, intBranchCityID, intBranchID, 0, 1, intUserID);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    this.Cursor = Cursors.WaitCursor;
                    tableLayoutPanel1.Enabled = false;
                    tableLayoutPanel1.BackColor = Color.Silver;

                    Cargo.frmDelivery frm = new Cargo.frmDelivery(txtLRNo.Text, intBranchID, intUserID, intBranchCityID, intCompanyID, ds, -1);
                    //frm.Width = this.Width - 200;
                    //frm.Height = this.Height - 200;
                    frm.ShowDialog();

                    txtLRNo.Text = "";
                    txtLRNo.Focus();

                    this.Cursor = Cursors.Default;

                    tableLayoutPanel1.BackColor = Color.White;
                    tableLayoutPanel1.Enabled = true;
                }
                else
                {
                    MessageBox.Show("No data found for given LRN.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void tableLayoutPanel11_Paint(object sender, PaintEventArgs e)
        {

        }

        private void radBRefund_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                if (txtLRNo.Text == "")
                {
                    MessageBox.Show("Please enter valid LRNo.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtLRNo.Focus();
                    return;
                }

                DataSet ds = new DataSet();

                ds = App_Code.Cargo.GetLuggageForRefund(intBranchCityID, intBranchID, txtLRNo.Text, intUserID);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    this.Cursor = Cursors.WaitCursor;
                    tableLayoutPanel1.Enabled = false;
                    tableLayoutPanel1.BackColor = Color.Silver;

                    Cargo.frmRefund frm = new Cargo.frmRefund(ds);
                    //frm.Width = this.Width - 200;
                    //frm.Height = this.Height - 200;
                    frm.ShowDialog();

                    txtLRNo.Text = "";
                    txtLRNo.Focus();

                    this.Cursor = Cursors.Default;

                    tableLayoutPanel1.BackColor = Color.White;
                    tableLayoutPanel1.Enabled = true;
                }
                else
                {
                    MessageBox.Show("No data found for given LRN.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void radBStatus_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                if (txtLRNo.Text == "")
                {
                    MessageBox.Show("Please enter valid LRNo.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtLRNo.Focus();
                    return;
                }

                DataSet ds = new DataSet();

                ds = App_Code.Cargo.GetStatus(intCompanyID, intBranchID, txtLRNo.Text, intUserID);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    this.Cursor = Cursors.WaitCursor;
                    tableLayoutPanel1.Enabled = false;
                    tableLayoutPanel1.BackColor = Color.Silver;

                    Cargo.frmStatus frm = new Cargo.frmStatus(txtLRNo.Text, ds);
                    //frm.Width = this.Width - 200;
                    //frm.Height = this.Height - 200;
                    frm.ShowDialog();

                    txtLRNo.Text = "";
                    txtLRNo.Focus();
                    this.Cursor = Cursors.Default;

                    tableLayoutPanel1.BackColor = Color.White;
                    tableLayoutPanel1.Enabled = true;
                }
                else
                {
                    MessageBox.Show("No data found for given LRN.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void radBReceiptPrint_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                if (txtLRNo.Text == "")
                {
                    MessageBox.Show("Please enter valid LRNo.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtLRNo.Focus();
                    return;
                }

                int _intBookingID = 0;
                string _StrLRNO = txtLRNo.Text;
                DataTable dtBooking = App_Code.Cargo.CargoBookingDetailsToPrint(_intBookingID, intBranchID, intUserID, _StrLRNO);

                if (dtBooking != null && dtBooking.Rows.Count > 0)
                {
                    Cargo.frmPrintTicket frmPrint = new Cargo.frmPrintTicket(_intBookingID, intBranchID, intUserID,
                                               _StrLRNO, dtBooking, false, strLaserPrinterName, strStickerPrinterName);
                    frmPrint.ShowDialog();
                    txtLRNo.Text = "";
                    txtLRNo.Focus();
                }
                else
                {
                    MessageBox.Show("No data found to print.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            catch (Exception ex)
            {

            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void radBDeliveryMemo_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                DataSet ds = new DataSet();

                ds = App_Code.Cargo.GetLuggageForDelivery("", intBranchCityID, intBranchID, 0, 2, intUserID);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    this.Cursor = Cursors.WaitCursor;
                    tableLayoutPanel1.Enabled = false;
                    tableLayoutPanel1.BackColor = Color.Silver;

                    Cargo.frmDeliveryMemo frm = new Cargo.frmDeliveryMemo(ds, this);
                    frm.Width = Convert.ToInt32(this.Width * 0.98);
                    frm.Height = Convert.ToInt32(this.Height * 0.90);
                    frm.ShowDialog();

                    txtLRNo.Text = "";
                    txtLRNo.Focus();

                    this.Cursor = Cursors.Default;

                    tableLayoutPanel1.BackColor = Color.White;
                    tableLayoutPanel1.Enabled = true;
                }
                else
                {
                    MessageBox.Show("No data found for Delivery.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void frmLuggageBooking_Enter(object sender, EventArgs e)
        {
            if (ToLoad)
            {
                if (!tableLayoutPanel1.Visible)
                    tableLayoutPanel1.Visible = true;
            }
            else
                ToLoad = true;
        }

        private void radBShow_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                if (txtLRNo.Text == "")
                {
                    MessageBox.Show("Please enter valid LRNo.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtLRNo.Focus();
                    return;
                }

                DataSet dsBooking = App_Code.Cargo.GetRptLRSearchResults(txtLRNo.Text, intBranchID, intCompanyID, DateTime.Now, DateTime.Now, intUserID);

                if (dsBooking != null && dsBooking.Tables.Count > 0 && dsBooking.Tables[0].Rows.Count > 0)
                {
                    if (!chkShowType.Checked && dsBooking.Tables.Count > 1 && dsBooking.Tables[1].Rows.Count > 0)
                    {
                        ShowLRNo(dsBooking);
                    }
                    else
                    {
                        Cargo.frmReportView frmRpt = new Cargo.frmReportView("Show", dsBooking);

                        frmRpt.ShowDialog();
                    }
                    //txtLRNo.Text = "";
                    txtLRNo.Focus();
                }
                else
                {
                    MessageBox.Show("No data found for give LR.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        public void ShowLRNo(DataSet dsBkgs)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                intShownBookingID = Convert.ToInt32(dsBkgs.Tables[0].Rows[0]["BookingID"]);
                radDDBookingCity.SelectedValue = dsBkgs.Tables[0].Rows[0]["BookingCityID"].ToString();
                radDDDestCity.SelectedValue = dsBkgs.Tables[0].Rows[0]["DestinationCityID"].ToString();
                radDDBookingBranch.SelectedValue = dsBkgs.Tables[0].Rows[0]["BookingBranchID"].ToString();
                radDDDestBranch.SelectedValue = dsBkgs.Tables[0].Rows[0]["DestinationBranchID"].ToString();
                radDPayType.SelectedValue = dsBkgs.Tables[0].Rows[0]["PaymentType"].ToString();
                string[] strDt = dsBkgs.Tables[0].Rows[0]["BookingDate"].ToString().Split(' ')[0].Split('-');

                radDDDD.Text = strDt[0];
                radDDMM.Text = strDt[1];
                radDDYY.Text = strDt[2];

                if (dsBkgs.Tables[0].Rows[0]["IsSenderParty"].ToString().ToUpper() == "1")
                {
                    chkIsPartySender.Checked = true;

                    for (int i = 0; i < radDPartySender.Items.Count; i++)
                    {
                        if (radDPartySender.Items[i].Value.ToString().Split('^')[0] == dsBkgs.Tables[0].Rows[0]["SendingPartyID"].ToString())
                        {
                            radDPartySender.Items[i].Selected = true;
                            break;
                        }
                    }
                }
                else
                {
                    txtNameSender.Text = dsBkgs.Tables[0].Rows[0]["Sender"].ToString();
                    txtMobileNo.Text = dsBkgs.Tables[0].Rows[0]["SenderMobileNo"].ToString();
                }
                if (dsBkgs.Tables[0].Rows[0]["IsReceivedParty"].ToString().ToUpper() == "1")
                {
                    chkIsPartyReceiver.Checked = true;
                    for (int i = 0; i < radDPartyReceiver.Items.Count; i++)
                    {
                        if (radDPartyReceiver.Items[i].Value.ToString().Split('^')[0] == dsBkgs.Tables[0].Rows[0]["RecPartyID"].ToString())
                        {
                            radDPartyReceiver.Items[i].Selected = true;
                            break;
                        }
                    }
                }
                else
                {
                    txtNameReceiver.Text = dsBkgs.Tables[0].Rows[0]["RecName"].ToString();
                    txtMobileNoReceiver.Text = dsBkgs.Tables[0].Rows[0]["RecMobileNo"].ToString();
                    txtAddressReceiver.Text = dsBkgs.Tables[0].Rows[0]["RecAddress"].ToString();
                }

                chkRecMobileGetData.Enabled = false;
                chkSenderMobileGetData.Enabled = false;
                txtFreightChg.Text = dsBkgs.Tables[0].Rows[0]["Freight"].ToString();
                txtDeliveryChg.Text = dsBkgs.Tables[0].Rows[0]["DeliveryCharges"].ToString();
                txtCollectionChg.Text = dsBkgs.Tables[0].Rows[0]["CollectionCharges"].ToString();
                txtCartage.Text = dsBkgs.Tables[0].Rows[0]["CartageAmount"].ToString();
                txtDocumentChg.Text = dsBkgs.Tables[0].Rows[0]["DocumentCharges"].ToString();
                txtInsurance.Text = dsBkgs.Tables[0].Rows[0]["Insurance"].ToString();
                txtServiceTax.Text = dsBkgs.Tables[0].Rows[0]["ServiceTaxAmount"].ToString();
                txtHamaliChg.Text = dsBkgs.Tables[0].Rows[0]["HamaliCharges"].ToString();

                radDModeofPayment.SelectedValue = dsBkgs.Tables[0].Rows[0]["ModeOfPayment"].ToString();

                txtSenderAddress.Text = dsBkgs.Tables[0].Rows[0]["SenderAddress"].ToString();

                string PayBillNo = dsBkgs.Tables[0].Rows[0]["PartyBillNo"].ToString();

                if (PayBillNo != "")
                {
                    string[] strPayBillDet = PayBillNo.Split('~');
                    txtBillNo.Text = strPayBillDet[0].Trim();

                    DateTime dtBill = DateTime.ParseExact(strPayBillDet[1], "dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    //Convert.ToDateTime(strPayBillDet[1]);
                    //radDDDDBill.Text = dtBill.Day.ToString();
                    //radDDMMBill.SelectedValue = dtBill.Month.ToString();
                    //radDDYYBill.Text = dtBill.Year.ToString();
                    dtBillNo.Value = Convert.ToDateTime(dtBill);
                    txtBillNo.ReadOnly = true;
                    dtBillNo.Enabled = false;
                }

                string EwayBillNo = dsBkgs.Tables[0].Rows[0]["EWayBillNo"].ToString();

                if (EwayBillNo != "" && EwayBillNo != "0")
                {
                    string[] strEwayBillDet = EwayBillNo.Split('~');
                    txtEwayBillNo.Text = strEwayBillDet[0].Trim();

                    DateTime dtEwayBillStart = DateTime.ParseExact(strEwayBillDet[1], "dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    DateTime dtEwayBillEnd = Common.GetServerTime(0, 0);

                    if(strEwayBillDet.Length > 2) 
                        dtEwayBillEnd = DateTime.ParseExact(strEwayBillDet[2], "dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture);

                    dtEwayBillStartDate = dtEwayBillStart;
                    dtEwayBillEndDate = dtEwayBillEnd;

                    txtEwayBillNo.ReadOnly = true;
                }

                txtComment.Text = dsBkgs.Tables[0].Rows[0]["Remarks"].ToString();
                txtSenderGSTN.Text = dsBkgs.Tables[0].Rows[0]["SenderGSTN"].ToString();
                txtReceiverGSTN.Text = dsBkgs.Tables[0].Rows[0]["ReceiverGSTN"].ToString();

                createDT();
                foreach (DataRow drw in dsBkgs.Tables[1].Rows)
                {
                    AddNewRow(drw["ConsignmentTypeID"].ToString() + "-" + drw["Rate"].ToString(),
                                drw["Description"].ToString(), drw["Qty"].ToString(),
                                drw["GoodsValue"].ToString(), drw["Rate"].ToString(), drw["Freight"].ToString(),
                                Convert.ToDouble(drw["ActualWeight"]));
                }

                fillGrid();
                TotalAmount();

                Boolean ShowForUpdate = true;

                if (radDDBookingBranch.SelectedValue.ToString() != Common.GetBranchID().ToString())
                {
                    if (intAllowUpdateOtherBranches == 0)
                        ShowForUpdate = false;
                }

                if (dsBkgs.Tables[0].Rows[0]["BookingBranchID"].ToString() != dsBkgs.Tables[0].Rows[0]["CurrentBranchID"].ToString())
                    ShowForUpdate = false;

                if (intAllowUpdate == 1 && intAllowUpdateAll == 0 && ShowForUpdate)
                {
                    //radDDBookingCity.Enabled = false;
                    //radDDBookingBranch.Enabled = false;
                    // Commented on 19 June
                    //radDDDestCity.Enabled = false;                
                    //radDDDestBranch.Enabled = false;
                    //txtSenderAddress.Enabled = false;
                    //txtAddressReceiver.ReadOnly = true;

                    radDPayType.Enabled = false;
                    chkIsPartySender.Enabled = false;

                    txtNameSender.ReadOnly = true;
                    txtMobileNo.ReadOnly = true;

                    txtNameReceiver.ReadOnly = true;
                    txtMobileNoReceiver.ReadOnly = true;

                    txtCollectionChg.ReadOnly = true;
                    txtDeliveryChg.ReadOnly = true;
                    txtCartage.ReadOnly = true;
                    txtDocumentChg.ReadOnly = true;
                    txtInsurance.ReadOnly = true;
                    txtServiceTax.ReadOnly = true;
                    txtComment.ReadOnly = true;

                    chkIsPartyReceiver.Enabled = false;
                    radDModeofPayment.Enabled = false;
                    radGridConsignItems.Enabled = false;
                    //radBSave.Enabled = false;

                    radBSave.Text = "Update";

                    txtBillNo.ReadOnly = true;
                    dtBillNo.Enabled = false;
                    //radDDDDBill.Enabled = false;
                    //radDDMMBill.Enabled = false;
                    //radDDYYBill.Enabled = false;

                    txtEwayBillNo.ReadOnly = true;
                    radbtnEwayBillDate.Enabled = false;
                }
                else if (intAllowUpdate == 1 && intAllowUpdateAll == 1 && ShowForUpdate)
                {
                    chkIsPartySender.Enabled = false;
                    chkIsPartyReceiver.Enabled = false;
                    radGridConsignItems.Enabled = false;

                    radDDBookingCity.Enabled = true;
                    radDDBookingBranch.Enabled = true;

                    radDDDestCity.Enabled = true;
                    radDDDestBranch.Enabled = true;

                    radDPartySender.Enabled = true;

                    if (!chkIsPartySender.Checked)
                    {
                        txtNameSender.Enabled = true;
                        txtMobileNo.Enabled = true;
                        txtSenderAddress.Enabled = true;
                    }

                    radDPartyReceiver.Enabled = true;

                    if (!chkIsPartyReceiver.Checked)
                    {
                        txtNameReceiver.Enabled = true;
                        txtMobileNoReceiver.Enabled = true;
                        txtAddressReceiver.Enabled = true;
                    }
                    radDPayType.Enabled = true;

                    txtCollectionChg.Enabled = true;
                    txtDeliveryChg.Enabled = true;
                    txtCartage.Enabled = true;
                    txtDocumentChg.Enabled = true;
                    txtInsurance.Enabled = true;
                    txtServiceTax.Enabled = false;
                    txtComment.Enabled = true;
                    radDModeofPayment.Enabled = false;

                    radBSave.Text = "Update";

                    setPaymentMode();
                }
                else
                {
                    radDDBookingCity.Enabled = false;
                    radDDBookingBranch.Enabled = false;
                    // Commented on 19 June
                    radDDDestCity.Enabled = false;
                    radDDDestBranch.Enabled = false;
                    txtSenderAddress.Enabled = false;
                    txtAddressReceiver.ReadOnly = true;

                    radDPayType.Enabled = false;
                    chkIsPartySender.Enabled = false;

                    txtNameSender.ReadOnly = true;
                    txtMobileNo.ReadOnly = true;
                    txtNameReceiver.ReadOnly = true;
                    txtMobileNoReceiver.ReadOnly = true;

                    txtCollectionChg.ReadOnly = true;
                    txtDeliveryChg.ReadOnly = true;
                    txtCartage.ReadOnly = true;
                    txtDocumentChg.ReadOnly = true;
                    txtInsurance.ReadOnly = true;
                    txtServiceTax.ReadOnly = true;
                    txtComment.ReadOnly = true;

                    chkIsPartyReceiver.Enabled = false;
                    radDModeofPayment.Enabled = false;
                    radGridConsignItems.Enabled = false;
                    //radBSave.Enabled = false;

                    //radBSave.Text = "Update";

                    txtBillNo.ReadOnly = true;
                    //radDDDDBill.Enabled = false;
                    //radDDMMBill.Enabled = false;
                    //radDDYYBill.Enabled = false;
                    dtBillNo.Enabled = false;
                    txtEwayBillNo.ReadOnly = true;
                    radbtnEwayBillDate.Enabled = false;
                }
            }
            catch (Exception ex)
            { }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void radGridConsignItems_CellValueChanged(object sender, GridViewCellEventArgs e)
        {
            IsValueChanged = true;
            //MessageBox.Show("hi");
        }

        private void radbUserWiseColl_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            Cargo.frmRptUserWiseCollections frm = new Cargo.frmRptUserWiseCollections("UWC");
            //Cargo.frmReportView frm = new Cargo.frmReportView("UWC");

            frm.Show();
            this.Cursor = Cursors.Default;
        }

        private void txtCollectionChg_Enter(object sender, EventArgs e)
        {
            TotalAmount();
        }

        private void tableLayoutPanel9_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void radBBranchTransfer_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            tableLayoutPanel1.Enabled = false;
            tableLayoutPanel1.BackColor = Color.Silver;
            //frmAdmin objFrmAdmin = (frmAdmin)Application.OpenForms["frmAdmin"];
            Cargo.frmBranchTransfer frm = new Cargo.frmBranchTransfer();
            //frm.Width = this.Width - 200;
            //frm.Height = this.Height - 200;
            frm.ShowDialog();
            this.Cursor = Cursors.Default;
            //objFrmAdmin.SetForm(frm);
            tableLayoutPanel1.BackColor = Color.White;
            tableLayoutPanel1.Enabled = true;
        }

        private void radBBranchReceipt_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            tableLayoutPanel1.Enabled = false;
            tableLayoutPanel1.BackColor = Color.Silver;
            //frmAdmin objFrmAdmin = (frmAdmin)Application.OpenForms["frmAdmin"];
            Cargo.frmBranchReceipt frm = new Cargo.frmBranchReceipt();
            //frm.Width = this.Width - 200;
            //frm.Height = this.Height - 200;
            frm.ShowDialog();
            this.Cursor = Cursors.Default;
            //objFrmAdmin.SetForm(frm);
            tableLayoutPanel1.BackColor = Color.White;
            tableLayoutPanel1.Enabled = true;
        }

        public void GetTransferAlerts()
        {
            try
            {
                //this.Cursor = Cursors.WaitCursor;

                DataSet ds = new DataSet();

                ds = App_Code.Cargo.GetBranchTransferedLuggageToReceive(0, intBranchID, intCompanyID, "".Trim(), 1, 1, "", intUserID, "", 0);
                timer1.Stop();
                timer1.Enabled = false;
                int intLastWidth = 0;

                for (int i = pnlMarqee.Controls.Count - 1; i >= 0; i--)
                {
                    if (pnlMarqee.Controls[i].Location.Y == 4)
                        pnlMarqee.Controls.RemoveAt(i);
                }
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    LinkLabel lbl = null;


                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        if (dr["VehicleNo"].ToString() == "")
                            continue;

                        string strLblName = "lnkLbl" + dr["BranchTransferID"];

                        if (pnlMarqee.Controls.Find(strLblName, false).Length == 0)
                        {

                            lbl = new LinkLabel();
                            lbl.AutoSize = true;
                            //lbl.Text = dr["FromBranch"].ToString() + " at " + dr["TransferDate"].ToString();
                            //lbl.Location = new Point(intLastWidth, 45);

                            lbl.LinkClicked += new LinkLabelLinkClickedEventHandler(linkLabel1_LinkClicked);
                            lbl.MouseHover += new EventHandler(linkLabel1_MouseHover);
                            lbl.MouseLeave += new EventHandler(linkLabel1_MouseLeave);

                            lbl.AutoSize = true;
                            lbl.Location = new System.Drawing.Point(intLastWidth, 4);
                            lbl.Name = "lnkLbl" + dr["BranchTransferID"].ToString();
                            //lbl.Size = new System.Drawing.Size(228, 13);
                            lbl.TabIndex = 0;
                            lbl.TabStop = true;
                            string typeOfDispatch = "Branch Transfer By :";

                            if (dr["Type"].ToString() == "D")
                                typeOfDispatch = "Dispatch By :";

                            lbl.Text = typeOfDispatch + dr["FromBranch"].ToString() + " " + dr["TransferDate"].ToString() + " in Vehicle " + dr["VehicleNo"] + ". LRNos:" + dr["LRNos"].ToString();
                            lbl.Tag = dr["FromBranchID"] + "||" + dr["VehicleNo"].ToString() + "||" + dr["TransferDate"].ToString() + "||" + dr["Type"].ToString();
                            //lbl.Tag = dr["VehicleNo"].ToString();

                            SizeF size = lbl.CreateGraphics().MeasureString(lbl.Text, lbl.Font);
                            int newWidth = Convert.ToInt32(size.Width);
                            int newHeight = Convert.ToInt32(size.Height);
                            lbl.Size = new System.Drawing.Size(newWidth, newHeight);

                            lbl.Refresh();

                            intLastWidth = intLastWidth + newWidth + 30;
                            //pnlMarqee.Width = intLastWidth + 20;


                            this.pnlMarqee.Controls.Add(lbl);

                            //pnlMarqee.Size = new System.Drawing.Size(intLastWidth, pnlMarqee.Height);
                            pnlMarqee.Refresh();
                        }
                    }

                    timer1.Enabled = true;
                    timer1.Start();
                    timerGetTransfered.Enabled = false;
                    timerGetTransfered.Stop();
                }
                else
                {
                    toGetTransfer = true;
                    timerGetTransfered.Enabled = true;
                    timerGetTransfered.Start();
                }
            }
            catch (Exception ex)
            {
                //ShowErrorMsg(ex.Message);
            }
            finally
            {
                // this.Cursor = Cursors.Default;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int currpos = xPos + pnlMarqee.Width;
            if (currpos == 0 || currpos == 1 || currpos == 2)
            {
                xPos = panel1.Width;
                GetTransferAlerts();
                GetShortReceiptAlerts();
                //repeat marquee
                pnlMarqee.Location = new System.Drawing.Point(xPos, 50);
            }
            else
            {

                pnlMarqee.Location = new System.Drawing.Point(xPos, 50);
                xPos = xPos - 2;
            }
        }

        private void timerGetTransfered_Tick(object sender, EventArgs e)
        {
            if (toGetTransfer)
                GetTransferAlerts();

            if (toGetShort)
                GetShortReceiptAlerts();

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            IsClicked = true;
            timer1.Stop();

            string[] tagVal = ((LinkLabel)sender).Tag.ToString().Replace("||", "~").Split('~');

            int intFromBranchID = Convert.ToInt32(tagVal[0]);
            string strVehicle = Convert.ToString(tagVal[1]);
            string strTransDate = Convert.ToString(tagVal[2]);
            string strType = Convert.ToString(tagVal[3]);

            //dr["FromBranchID"] + "||" + dr["VehicleNo"].ToString() + "||" + dr["TransferDate"].ToString();

            this.Cursor = Cursors.WaitCursor;
            tableLayoutPanel1.Enabled = false;
            tableLayoutPanel1.BackColor = Color.Silver;
            //frmAdmin objFrmAdmin = (frmAdmin)Application.OpenForms["frmAdmin"];
            if (strType == "T")
            {
                Cargo.frmBranchReceipt frm = new Cargo.frmBranchReceipt(strVehicle, intFromBranchID, strTransDate);
                //frm.Width = this.Width - 200;
                //frm.Height = this.Height - 200;
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    xPos = panel1.Width;
                    GetTransferAlerts();
                }
            }
            else
            {
                Cargo.frmReceipt frm = new Cargo.frmReceipt(strVehicle, intFromBranchID, strTransDate);
                //frm.Width = this.Width - 200;
                //frm.Height = this.Height - 200;
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    xPos = panel1.Width;
                    GetTransferAlerts();
                }
            }
            this.Cursor = Cursors.Default;
            //objFrmAdmin.SetForm(frm);
            tableLayoutPanel1.BackColor = Color.White;
            tableLayoutPanel1.Enabled = true;


            timer1.Start();
            ((LinkLabel)sender).LinkColor = Color.Blue;
            IsClicked = false;
        }

        private void linkLabel1_MouseHover(object sender, EventArgs e)
        {
            timer1.Stop();
            ((LinkLabel)sender).LinkColor = Color.Red;

        }

        private void linkLabel1_MouseLeave(object sender, EventArgs e)
        {
            if (!IsClicked)
            {
                ((LinkLabel)sender).LinkColor = Color.Blue;
                timer1.Start();
            }
        }

        private void timerGetShortReceived_Tick(object sender, EventArgs e)
        {
            GetShortReceiptAlerts();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            int currpos = xPos1 + pnlMarqeeShortReceipt.Width;
            if (currpos == 0 || currpos == 1 || currpos == 2)
            {
                xPos = panel1.Width;
                GetShortReceiptAlerts();
                //repeat marquee
                pnlMarqeeShortReceipt.Location = new System.Drawing.Point(xPos1, 75);
            }
            else
            {

                pnlMarqeeShortReceipt.Location = new System.Drawing.Point(xPos1, 75);
                xPos1 = xPos1 - 2;
            }
        }

        public void GetShortReceiptAlerts()
        {
            try
            {
                //this.Cursor = Cursors.WaitCursor;

                DataSet ds = new DataSet();

                for (int i = pnlMarqee.Controls.Count - 1; i >= 0; i--)
                {
                    if (pnlMarqee.Controls[i].Location.Y == 30)
                        pnlMarqee.Controls.RemoveAt(i);
                }

                ds = App_Code.Cargo.GetBranchTransferedLuggageToReceive(0, intBranchID, intCompanyID, "".Trim(), 1, 100, "", intUserID, "", 0);
                timer2.Stop();
                timer2.Enabled = false;
                int intLastWidth = 0;


                if (ds != null && ds.Tables.Count > 0)
                {
                    LinkLabel lbl = null;

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            if (dr["VehicleNo"].ToString() == "")
                                continue;

                            string strLblName = "lnkLbl" + dr["BranchTransferID"];

                            if (pnlMarqeeShortReceipt.Controls.Find(strLblName, false).Length == 0)
                            {

                                lbl = new LinkLabel();
                                lbl.AutoSize = true;
                                lbl.LinkColor = Color.Red;
                                //lbl.Text = dr["FromBranch"].ToString() + " at " + dr["TransferDate"].ToString();
                                //lbl.Location = new Point(intLastWidth, 45);

                                //lbl.LinkClicked += new LinkLabelLinkClickedEventHandler(linkLabel1_LinkClicked);
                                //lbl.MouseHover += new EventHandler(linkLabel1_MouseHover);
                                //lbl.MouseLeave += new EventHandler(linkLabel1_MouseLeave);

                                lbl.AutoSize = true;
                                lbl.Location = new System.Drawing.Point(intLastWidth, 30);
                                lbl.Name = "lnkLbl" + dr["BranchTransferID"].ToString();
                                //lbl.Size = new System.Drawing.Size(228, 13);
                                lbl.TabIndex = 0;
                                lbl.TabStop = true;

                                string typeOfDispatch = "Branch Transfered : ";

                                if (dr["Type"].ToString() == "D")
                                    typeOfDispatch = "Dispatched : ";

                                lbl.Text = typeOfDispatch + "Short Receipt by " + dr["ToBranch"].ToString() + " " + dr["UpdateDate"].ToString() +
                                            " in Vehicle " + dr["VehicleNo"] + ". LRNos:" + dr["LRNos"].ToString();
                                //lbl.Tag = dr["FromBranchID"] + "||" + dr["VehicleNo"].ToString() + "||" + dr["TransferDate"].ToString();
                                //lbl.Tag = dr["VehicleNo"].ToString();

                                SizeF size = lbl.CreateGraphics().MeasureString(lbl.Text, lbl.Font);
                                int newWidth = Convert.ToInt32(size.Width);
                                int newHeight = Convert.ToInt32(size.Height);
                                lbl.Size = new System.Drawing.Size(newWidth, newHeight);

                                lbl.Refresh();

                                intLastWidth = intLastWidth + newWidth + 30;
                                //pnlMarqee.Width = intLastWidth + 20;


                                this.pnlMarqee.Controls.Add(lbl);

                                //pnlMarqeeShortReceipt.Size = new System.Drawing.Size(intLastWidth, pnlMarqee.Height);
                                pnlMarqee.Refresh();
                            }
                        }
                    }

                    if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                    {
                        foreach (DataRow drw in ds.Tables[1].Rows)
                        {
                            if (drw["LRNo"].ToString() == "")
                                continue;

                            string strLblName = "lnkLbl" + drw["LRNo"];

                            if (pnlMarqeeShortReceipt.Controls.Find(strLblName, false).Length == 0)
                            {

                                lbl = new LinkLabel();
                                lbl.AutoSize = true;
                                lbl.LinkColor = Color.Green;
                                //lbl.Text = dr["FromBranch"].ToString() + " at " + dr["TransferDate"].ToString();
                                //lbl.Location = new Point(intLastWidth, 45);

                                //lbl.LinkClicked += new LinkLabelLinkClickedEventHandler(linkLabel1_LinkClicked);
                                //lbl.MouseHover += new EventHandler(linkLabel1_MouseHover);
                                //lbl.MouseLeave += new EventHandler(linkLabel1_MouseLeave);

                                lbl.AutoSize = true;
                                lbl.Location = new System.Drawing.Point(intLastWidth, 30);
                                lbl.Name = "lnkLbl" + drw["LRNo"].ToString();
                                //lbl.Size = new System.Drawing.Size(228, 13);
                                lbl.TabIndex = 0;
                                lbl.TabStop = true;

                                string typeOfDispatch = "At Receipt : ";

                                if (drw["Type"].ToString() == "BR")
                                    typeOfDispatch = "At Branch Receipt : ";

                                lbl.Text = typeOfDispatch + "LR No   " + drw["LRNo"].ToString() + " " + drw["ReceiptDateTime"].ToString() +
                                            ",   " + drw["ShortQty"] + " found short of " + drw["Items"].ToString() + ".";
                                //lbl.Tag = dr["FromBranchID"] + "||" + dr["VehicleNo"].ToString() + "||" + dr["TransferDate"].ToString();
                                //lbl.Tag = dr["VehicleNo"].ToString();

                                SizeF size = lbl.CreateGraphics().MeasureString(lbl.Text, lbl.Font);
                                int newWidth = Convert.ToInt32(size.Width);
                                int newHeight = Convert.ToInt32(size.Height);
                                lbl.Size = new System.Drawing.Size(newWidth, newHeight);

                                lbl.Refresh();

                                intLastWidth = intLastWidth + newWidth + 30;
                                //pnlMarqee.Width = intLastWidth + 20;


                                this.pnlMarqee.Controls.Add(lbl);

                                //pnlMarqeeShortReceipt.Size = new System.Drawing.Size(intLastWidth, pnlMarqee.Height);
                                pnlMarqee.Refresh();
                            }
                        }
                    }
                    timer1.Enabled = true;
                    timer1.Start();
                    timerGetTransfered.Enabled = false;
                    timerGetTransfered.Stop();
                }
                else
                {
                    toGetShort = true;
                    timerGetTransfered.Enabled = true;
                    timerGetTransfered.Start();
                }
            }
            catch (Exception ex)
            {
                //ShowErrorMsg(ex.Message);
            }
            finally
            {
                // this.Cursor = Cursors.Default;
            }
        }

        private void pnlMarqee_Paint(object sender, PaintEventArgs e)
        {

        }

        private void txtDeliveryChg_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = Common.AllowNumeric(e);
        }

        private void chkIsCollection_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chkCollection = ((CheckBox)sender);

            if (chkCollection.Checked)
            {
                radDCollectionType.Visible = true;
                radDCollectionType.SelectedIndex = 0;
                lblCollChgBx.Visible = true;
                txtCollectionChg.Visible = true;
                radDCollectionType.Focus();
            }
            else
            {
                lblCollChgBx.Visible = false;
                txtCollectionChg.Visible = false;
                radDCollectionType.Visible = false;
                radDCollectionType.SelectedIndex = 0;
                //if (txtBillNo.Visible)
                //    txtBillNo.Focus();
                //else if (chkIsPartyReceiver.Visible)
                //    chkIsPartyReceiver.Focus();
                //else
                //    txtNameReceiver.Focus();
            }
        }

        private void chkIsDelivery_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chkDelivery = ((CheckBox)sender);

            if (chkDelivery.Checked)
            {
                lblDeliveryChg.Visible = true;
                txtDeliveryChg.Visible = true;
                raddDeliveryType.Visible = true;
                raddDeliveryType.SelectedIndex = 0;
                raddDeliveryType.Focus();
            }
            else
            {
                lblDeliveryChg.Visible = false;
                txtDeliveryChg.Visible = false;
                raddDeliveryType.Visible = false;
                raddDeliveryType.SelectedIndex = 0;
            }
        }

        private void chkIsDelivery_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                radGridConsignItems.Focus();
            }
        }

        private void raddDeliveryType_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                radGridConsignItems.Focus();
            }
        }

        private void txtMobileNo_Validating(object sender, CancelEventArgs e)
        {
            //try
            //{
            //    TextBox txtBx = (TextBox)sender;

            //    if (txtBx.Name.Contains("txtMobileNo"))
            //    {
            //        if (txtBx.Text == "")
            //        {
            //            e.Cancel = true;
            //            MessageBox.Show("Please enter Sender Mobile No.");
            //        }
            //        else if (txtBx.Text.Length != 10)
            //        {
            //            e.Cancel = true;
            //            MessageBox.Show("Mobile No. should have 10 digit.");
            //        }
            //        else
            //        {
            //            e.Cancel = false;
            //            this.Cursor = Cursors.WaitCursor;
            //            DataSet ds = new DataSet();

            //            ds = App_Code.Cargo.GetNameAndAddress("S", txtMobileNo.Text, "", intCompanyID, intBranchID, intUserID);

            //            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            //            {
            //                if (txtSenderAddress.Visible)
            //                    txtSenderAddress.Text = ds.Tables[0].Rows[0]["SenderAddress"].ToString();

            //                txtNameSender.Text = ds.Tables[0].Rows[0]["Sender"].ToString();
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{ }
            //finally
            //{
            //    this.Cursor = Cursors.Default;
            //}
        }

        private void txtMobileNoReceiver_Validating(object sender, CancelEventArgs e)
        {
            //try
            //{
            //    TextBox txtBx = (TextBox)sender;

            //    if (txtBx.Name.Contains("txtMobileNoReceiver"))
            //    {
            //        if (txtBx.Text == "")
            //        {
            //            e.Cancel = true;
            //            MessageBox.Show("Please enter Receiver Mobile No.");
            //        }
            //        else if (txtBx.Text.Length != 10)
            //        {
            //            e.Cancel = true;
            //            MessageBox.Show("Mobile No. should have 10 digit.");
            //        }
            //        else
            //        {
            //            e.Cancel = false;
            //            this.Cursor = Cursors.WaitCursor;
            //            DataSet ds = new DataSet();

            //            ds = App_Code.Cargo.GetNameAndAddress("S", txtMobileNoReceiver.Text, "", intCompanyID, intBranchID, intUserID);

            //            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            //            {
            //                if (txtSenderAddress.Visible)
            //                    txtAddressReceiver.Text = ds.Tables[0].Rows[0]["RecAddress"].ToString();

            //                txtNameReceiver.Text = ds.Tables[0].Rows[0]["RecName"].ToString();
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{ }
            //finally
            //{
            //    this.Cursor = Cursors.Default;
            //}
        }

        private void txtMobileNoReceiver_Leave(object sender, EventArgs e)
        {
            if (txtMobileNoReceiver.Text.ToString().Length != 0 && txtMobileNoReceiver.Text.ToString().Length != 10)
            {
                MessageBox.Show("Please enter valid Receiver Mobile number.");
                txtMobileNoReceiver.Focus();
            }

            try
            {
                if (txtMobileNoReceiver.Text.ToString().Length != 0 && txtMobileNoReceiver.Text.ToString().Length != 10)
                {
                    MessageBox.Show("Please enter valid Receiver Mobile number.");
                    txtMobileNoReceiver.Focus();
                }
                else
                {
                    long EnteredRecMobNumber = Convert.ToInt64(txtMobileNoReceiver.Text);

                    if (((EnteredRecMobNumber / (EnteredRecMobNumber % 10)) == 1111111111)) //|| (txtMobileNoReceiver.Text[0] - '0') < 7
                    {
                        MessageBox.Show("Invalid Receiver Mobile Number. Please re-enter.");
                        txtMobileNoReceiver.Focus();
                        return;
                    }
                }
            }
            catch (Exception ex)
            { }
        }

        private void radDCollectionType_SelectedIndexChanged(object sender, Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
        {
            int SelectedIndex = ((RadDropDownList)sender).SelectedIndex;

            if (SelectedIndex == 2)
            {
                txtCollectionChg.Enabled = false;
                txtCollectionChg.Text = "0";
            }
            else
            {
                txtCollectionChg.Enabled = true;
                txtCollectionChg.Text = "0";
            }
        }

        private void raddDeliveryType_SelectedIndexChanged(object sender, Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
        {
            int SelectedIndex = ((RadDropDownList)sender).SelectedIndex;

            if (SelectedIndex == 2)
            {
                txtDeliveryChg.Enabled = false;
                txtDeliveryChg.Text = "0";
            }
            else
            {
                txtDeliveryChg.Enabled = true;
                txtDeliveryChg.Text = "0";
            }
        }

        private void chkRecMobileGetData_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                CheckBox chkBx = ((CheckBox)sender);

                if (chkBx.Checked)
                {
                    if (txtMobileNoReceiver.Text == "")
                    {
                        txtMobileNoReceiver.Text = "";
                    }
                    else
                    {
                        GetCustomerDetails("R");
                    }
                }
                else
                {
                    txtAddressReceiver.Text = "";
                    txtNameReceiver.Text = "";
                    txtReceiverGSTN.Text = "";
                    SetGSTPaidBy();
                }
            }
            catch (Exception ex)
            { }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void chkSenderMobileGetData_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                CheckBox chkBx = ((CheckBox)sender);

                if (chkBx.Checked)
                {
                    if (txtMobileNo.Text == "")
                    {
                        txtMobileNo.Text = "";
                    }
                    else
                    {
                        GetCustomerDetails("S");
                    }
                }
                else
                {
                    txtSenderAddress.Text = "";
                    txtNameSender.Text = "";
                    txtSenderGSTN.Text = "";
                    SetGSTPaidBy();
                }
            }
            catch (Exception ex)
            { }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void radBOfflineBooking_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            tableLayoutPanel1.Enabled = false;
            tableLayoutPanel1.BackColor = Color.Silver;
            //frmAdmin objFrmAdmin = (frmAdmin)Application.OpenForms["frmAdmin"];
            Cargo.frmLuggageBookingManual frm = new Cargo.frmLuggageBookingManual();
            //frm.Width = this.Width - 200;
            //frm.Height = this.Height - 200;
            frm.ShowDialog();
            this.Cursor = Cursors.Default;
            //objFrmAdmin.SetForm(frm);
            tableLayoutPanel1.BackColor = Color.White;
            tableLayoutPanel1.Enabled = true;
        }

        private void radBDailyExpense_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                tableLayoutPanel1.Enabled = false;
                CRS2011DeskApp.frmDailyExpenses objFrm = new CRS2011DeskApp.frmDailyExpenses();
                objFrm.Size = new System.Drawing.Size(Convert.ToInt32(this.Width / 2), Convert.ToInt32(this.Height / 2));
                objFrm.WindowState = FormWindowState.Normal;
                objFrm.StartPosition = FormStartPosition.CenterScreen;
                objFrm.ShowDialog();
                tableLayoutPanel1.Enabled = true;
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void ShowError(string strErr)
        {
            MessageBox.Show(strErr, "Oops..", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void radDDCrossingCity_SelectedIndexChanged(object sender, Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
        {
            try
            {
                if (radDDCrossingCity.SelectedIndex == -1)
                    return;

                this.Cursor = Cursors.WaitCursor;
                FillCrossingBranch();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }

        }

        private void chkCrossing_CheckedChanged(object sender, EventArgs e)
        {
            if (chkCrossing.Checked)
            {
                if (intCompanyID == 1 || is_crossing_company)
                {
                    lblCrossingCity.Visible = true;
                    lblCrossingBranch.Visible = true;
                    radDDCrossingCity.Visible = true;
                    radDDCrossingBranch.Visible = true;
                }
            }
            else
            {
                lblCrossingCity.Visible = false;
                lblCrossingBranch.Visible = false;
                radDDCrossingCity.Visible = false;
                radDDCrossingBranch.Visible = false;

            }
        }

        private void btnChangeVehicle_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                tableLayoutPanel1.Enabled = false;
                CRS2011DeskApp.Cargo.frmChangeVehicle frm = new CRS2011DeskApp.Cargo.frmChangeVehicle();
                //objFrm.Size = new System.Drawing.Size(Convert.ToInt32(this.Width / 2), Convert.ToInt32(this.Height / 2));
                frm.WindowState = FormWindowState.Normal;
                frm.StartPosition = FormStartPosition.CenterScreen;
                frm.ShowDialog();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                tableLayoutPanel1.Enabled = true;
            }
        }

        public void SetSelectedLR(string strLR)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                strSelectedLRNo = strLR;

            }
            catch (Exception ex)
            {

            }
            finally
            {
                this.Cursor = Cursors.Default;
            }

        }

        private void txtSenderGSTN_Leave(object sender, EventArgs e)
        {

            string sender_gstn = txtSenderGSTN.Text.ToUpper();
            int gstn_len = sender_gstn.Length;
            string payType = radDPayType.SelectedValue.ToString();

            if (gstn_len != 0 && gstn_len != 15)
            {
                MessageBox.Show("Invalid GSTN. GSTN should be in '22AAAAA0000A1Z5' format.");
                txtSenderGSTN.Focus();
                return;
            }
            else if (gstn_len == 0)
            {
                if (lblStaxPaidBy.Visible && radDDCStaxPaidBy.Visible && is_stax_company)
                {
                    if (GSTType == 1)
                    {
                        if (txtReceiverGSTN.Text.Trim().Length != 0)
                            radDDCStaxPaidBy.SelectedValue = 4; //consignee
                        else
                            radDDCStaxPaidBy.SelectedValue = 2; //transporter
                    }
                    else
                    {
                        if (radDPayType.SelectedValue.ToString() != "2")
                        {
                            radDDCStaxPaidBy.SelectedValue = 2; //transporter
                        }
                    }
                }
            }
            else
            {
                if (Char.IsDigit(sender_gstn[0])
                    && Char.IsDigit(sender_gstn[1])
                    && Char.IsLetter(sender_gstn[2])
                    && Char.IsLetter(sender_gstn[3])
                    && Char.IsLetter(sender_gstn[4])
                    && Char.IsLetter(sender_gstn[5])
                    && Char.IsLetter(sender_gstn[6])
                    && Char.IsDigit(sender_gstn[7])
                    && Char.IsDigit(sender_gstn[8])
                    && Char.IsDigit(sender_gstn[9])
                    && Char.IsDigit(sender_gstn[10])
                    && Char.IsLetter(sender_gstn[11])
                    && Char.IsDigit(sender_gstn[12])
                    && sender_gstn[13].ToString() == "Z"
                    && Char.IsLetterOrDigit(sender_gstn[14]))
                {
                    if (lblStaxPaidBy.Visible && radDDCStaxPaidBy.Visible && is_stax_company)
                    {
                        if (GSTType == 1)
                        {
                            if (txtReceiverGSTN.Text.Trim().Length != 0)
                            {
                                if (radDPayType.SelectedValue.ToString() != "2")
                                    radDDCStaxPaidBy.SelectedValue = 3; //consigner
                                else
                                    radDDCStaxPaidBy.SelectedValue = 4; //consignee
                            }
                            else
                            {
                                radDDCStaxPaidBy.SelectedValue = 3; //consigner
                            }
                        }
                        else if (GSTType == 2)
                        {
                            if (radDPayType.SelectedValue.ToString() != "2")
                            {
                                radDDCStaxPaidBy.SelectedValue = 3;
                            }
                        }
                    }
                    //if (payType != "2")
                    //{
                    //    radDDCStaxPaidBy.SelectedValue = "3";
                    //}
                }
                else
                {
                    MessageBox.Show("Invalid GSTN. GSTN should be in '22AAAAA0000A1Z5' format.");
                    txtSenderGSTN.Focus();
                    return;
                }
            }
        }

        private void txtReceiverGSTN_Leave(object sender, EventArgs e)
        {
            string receiver_gstn = txtReceiverGSTN.Text.ToUpper();
            int gstn_len = receiver_gstn.Length;
            string payType = radDPayType.SelectedValue.ToString();

            if (gstn_len != 0 && gstn_len != 15)
            {
                MessageBox.Show("Invalid GSTN. GSTN should be in '22AAAAA0000A1Z5' format.");
                txtReceiverGSTN.Focus();
                return;
            }
            else if (gstn_len == 0)
            {
                if (lblStaxPaidBy.Visible && radDDCStaxPaidBy.Visible && is_stax_company)
                {
                    if (GSTType == 1)
                    {
                        if (txtSenderGSTN.Text.Trim().Length != 0)
                            radDDCStaxPaidBy.SelectedValue = 3; //consigner
                        else
                            radDDCStaxPaidBy.SelectedValue = 2; //transporter
                    }
                    else
                    {
                        if (radDPayType.SelectedValue.ToString() == "2")
                        {
                            radDDCStaxPaidBy.SelectedValue = 2; //transporter
                        }
                    }
                }
            }
            else
            {
                if (Char.IsDigit(receiver_gstn[0])
                    && Char.IsDigit(receiver_gstn[1])
                    && Char.IsLetter(receiver_gstn[2])
                    && Char.IsLetter(receiver_gstn[3])
                    && Char.IsLetter(receiver_gstn[4])
                    && Char.IsLetter(receiver_gstn[5])
                    && Char.IsLetter(receiver_gstn[6])
                    && Char.IsDigit(receiver_gstn[7])
                    && Char.IsDigit(receiver_gstn[8])
                    && Char.IsDigit(receiver_gstn[9])
                    && Char.IsDigit(receiver_gstn[10])
                    && Char.IsLetter(receiver_gstn[11])
                    && Char.IsDigit(receiver_gstn[12])
                    && receiver_gstn[13].ToString() == "Z"
                    && Char.IsLetterOrDigit(receiver_gstn[14]))
                {
                    if (lblStaxPaidBy.Visible && radDDCStaxPaidBy.Visible && is_stax_company)
                    {
                        if (GSTType == 1)
                        {
                            if (txtSenderGSTN.Text.Trim().Length != 0)
                            {
                                if (radDPayType.SelectedValue.ToString() != "2")
                                    radDDCStaxPaidBy.SelectedValue = 3;
                                else
                                    radDDCStaxPaidBy.SelectedValue = 4;
                            }
                            else
                            {
                                radDDCStaxPaidBy.SelectedValue = 4;
                            }
                        }
                        else if (GSTType == 2)
                        {
                            if (radDPayType.SelectedValue.ToString() == "2")
                            {
                                radDDCStaxPaidBy.SelectedValue = 4;
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Invalid GSTN. GSTN should be in '22AAAAA0000A1Z5' format.");
                    txtReceiverGSTN.Focus();
                    return;
                }
            }
        }

        private void radDDCStaxPaidBy_SelectedIndexChanged(object sender, Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
        {
            txtServiceTax.Text = "0";
            TotalAmount();
        }

        private void txtMobileNo_Leave(object sender, EventArgs e)
        {
            try
            {
                if (txtMobileNo.Text.ToString().Length != 0 && txtMobileNo.Text.ToString().Length != 10)
                {
                    MessageBox.Show("Please enter valid Sender Mobile number.");
                    txtMobileNo.Focus();
                }
                else
                {
                    long EnteredSenMobNumber = Convert.ToInt64(txtMobileNo.Text);

                    if (((EnteredSenMobNumber / (EnteredSenMobNumber % 10)) == 1111111111)) //|| (txtMobileNo.Text[0] - '0') < 7
                    {
                        MessageBox.Show("Invalid Sender Mobile Number. Please re-enter.");
                        txtMobileNo.Focus();
                        return;
                    }
                }
            }
            catch (Exception ex)
            { }
        }

        private void txtHamaliChg_Leave(object sender, EventArgs e)
        {
            try
            {
                bool IsLrwise = false;

                if (!Common.IsMTPLAdminUser() && !Common.IsCompanyAdminUser() && !Common.IsBranchAdminUser() && (intAllowHamaliEdit != 2))
                {

                    if (intAllowHamaliEdit == 1)
                    {
                        int tot_quantity = 0;
                        for (int i = 0; i < radGridConsignItems.Rows.Count; i++)
                        {
                            int quantity = Convert.ToInt32(radGridConsignItems.Rows[rowIndex].Cells["Qty"].Value);
                            tot_quantity += quantity;
                        }

                        if (intMaxHamaliPerUnit == 0)
                            IsLrwise = true;
                        else if (intMaxHamaliPerLR == 0)
                            IsLrwise = false;
                        else if ((tot_quantity * intMaxHamaliPerUnit) < (intMaxHamaliPerLR * radGridConsignItems.Rows.Count))
                            IsLrwise = false;
                        else
                            IsLrwise = true;

                        if (IsLrwise)
                        {
                            if (Convert.ToInt32(txtHamaliChg.Text) > (intMaxHamaliPerLR * radGridConsignItems.Rows.Count))
                            {
                                MessageBox.Show("Error : Hamali Charge per LR can not be greater than " + intMaxHamaliPerLR);
                                txtHamaliChg.Focus();
                                return;
                            }
                        }
                        else
                        {
                            if (Convert.ToInt32(txtHamaliChg.Text) > (tot_quantity * intMaxHamaliPerUnit))
                            {
                                MessageBox.Show("Error : Hamali Charge can not be greater than (No. of Units x " + intMaxHamaliPerUnit + ") = " + tot_quantity + " x " + intMaxHamaliPerUnit + " = " + (tot_quantity * intMaxHamaliPerUnit));
                                txtHamaliChg.Focus();
                                return;
                            }
                        }
                    }
                }
            }
            catch
            { }
        }

        private void lnkimageandID_Click(object sender, EventArgs e)
        {
            Cargo.frmCaptureImageandID frm = new Cargo.frmCaptureImageandID(this, intIDProofTypeID, strIDProofNo, strSenderImageName, strIDProofImageName);
            frm.ShowDialog();

            if (strSenderImageName != "")
                lnkimageandID.Text = "Edit Captured Image and ID";
            else
                lnkimageandID.Text = "Capture Image and ID";
        }

        public void SetImageandID(int _intIDProofTypeID, string _strIDProofNo, string _strSenderImageName, string _strIDProofImageName)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                intIDProofTypeID = _intIDProofTypeID;
                strIDProofNo = _strIDProofNo;
                strSenderImageName = _strSenderImageName;
                strIDProofImageName = _strIDProofImageName;
            }
            catch (Exception ex)
            {

            }
            finally
            {
                this.Cursor = Cursors.Default;
            }

        }

        private void radBtnCommonLR_Click(object sender, EventArgs e)
        {
            Cargo.frmCommonLRSearch frm = new Cargo.frmCommonLRSearch(false);
            frm.Width = Convert.ToInt32(this.Width * 0.95);
            frm.Height = Convert.ToInt32(this.Height * 0.90);
            frm.ShowDialog();
        }

        private void GetCustomerDetails(string CustomerCode)
        {
            try
            {
                if (CustomerCode == "S")
                {
                    this.Cursor = Cursors.WaitCursor;
                    DataSet ds = new DataSet();

                    ds = App_Code.Cargo.GetNameAndAddress("S", txtMobileNo.Text, "", intCompanyID, intBranchID, intUserID);

                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        if (txtSenderAddress.Visible)
                            txtSenderAddress.Text = ds.Tables[0].Rows[0]["SenderAddress"].ToString();

                        if (ds.Tables[0].Rows[0]["SenderGSTN"].ToString().Trim() != "")
                        {
                            txtSenderGSTN.Text = ds.Tables[0].Rows[0]["SenderGSTN"].ToString();

                            if (is_stax_company)
                                SetGSTPaidBy();
                        }

                        txtNameSender.Text = ds.Tables[0].Rows[0]["Sender"].ToString();
                    }
                }
                else if (CustomerCode == "R")
                {
                    this.Cursor = Cursors.WaitCursor;
                    DataSet ds = new DataSet();

                    ds = App_Code.Cargo.GetNameAndAddress("R", txtMobileNoReceiver.Text, "", intCompanyID, intBranchID, intUserID);

                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        if (txtAddressReceiver.Visible)
                            txtAddressReceiver.Text = ds.Tables[0].Rows[0]["RecAddress"].ToString();

                        if (ds.Tables[0].Rows[0]["ReceiverGSTN"].ToString().Trim() != "")
                        {
                            txtReceiverGSTN.Text = ds.Tables[0].Rows[0]["ReceiverGSTN"].ToString();

                            if (is_stax_company)
                                SetGSTPaidBy();
                        }

                        txtNameReceiver.Text = ds.Tables[0].Rows[0]["RecName"].ToString();
                    }
                }

            }
            catch (Exception ex)
            {

            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void SetGSTPaidBy()
        {
            try
            {
                txtServiceTax.Text = "0";

                if (lblStaxPaidBy.Visible && radDDCStaxPaidBy.Visible && is_stax_company)
                {
                    if (GSTType == 1)//GST type  -- STA
                    {
                        if (txtSenderGSTN.Text.Trim() != "")
                        {
                            if (txtReceiverGSTN.Text.Trim() != "")
                            {
                                if (radDPayType.SelectedValue.ToString() != "2")
                                    radDDCStaxPaidBy.SelectedValue = 3; //Consigner
                                else
                                    radDDCStaxPaidBy.SelectedValue = 4;  //Consignee
                            }
                            else
                            {
                                radDDCStaxPaidBy.SelectedValue = 3; //Consigner
                            }
                        }
                        else
                        {
                            if (txtReceiverGSTN.Text.ToString().Trim() == "")
                                radDDCStaxPaidBy.SelectedValue = 2; //Transporter
                        }


                        if (txtReceiverGSTN.Text.Trim() != "")
                        {
                            if (txtSenderGSTN.Text.Trim() != "")
                            {
                                if (radDPayType.SelectedValue.ToString() != "2")
                                    radDDCStaxPaidBy.SelectedValue = 3; //Consigner
                                else
                                    radDDCStaxPaidBy.SelectedValue = 4;  //Consignee
                            }
                            else
                                radDDCStaxPaidBy.SelectedValue = 4; //Consignee
                        }
                        else
                        {
                            if (txtSenderGSTN.Text.ToString().Trim() == "") // && radDPayType.SelectedValue.ToString() != "2"
                                radDDCStaxPaidBy.SelectedValue = 2; //Transporter
                        }
                    }
                    else //GST type  -- hans
                    {
                        if (txtSenderGSTN.Text.Trim() != "")
                        {
                            if (radDPayType.SelectedValue.ToString() != "2")
                                radDDCStaxPaidBy.SelectedValue = 3; //Consigner
                            else
                            {
                                if (txtReceiverGSTN.Text.Trim() != "")
                                    radDDCStaxPaidBy.SelectedValue = 4; //Consignee
                                else
                                    radDDCStaxPaidBy.SelectedValue = 3; //Consigner
                            }
                        }
                        else
                        {
                            if (radDPayType.SelectedValue.ToString() != "2")
                                radDDCStaxPaidBy.SelectedValue = 2; //Transporter
                        }

                        if (txtReceiverGSTN.Text.Trim() != "")
                        {
                            if (radDPayType.SelectedValue.ToString() == "2")
                                radDDCStaxPaidBy.SelectedValue = 4; //Consignee
                            else
                            {
                                if (txtSenderGSTN.Text.Trim() != "")
                                    radDDCStaxPaidBy.SelectedValue = 3; //Consigner
                                else
                                    radDDCStaxPaidBy.SelectedValue = 2; //Transporter
                            }
                        }
                        else
                        {
                            if (radDPayType.SelectedValue.ToString() == "2")
                                radDDCStaxPaidBy.SelectedValue = 2; //Transporter
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void SetVisibilityOfBillAndEwayBillNo(int intIsBillNo, int intIsEwayBillNo)
        {
            if (intIsBillNo == 1 && intIsEwayBillNo == 1)
            {
                lblBillNo.Visible = true;
                txtBillNo.Visible = true;
                dtBillNo.Visible = true;

                lblewaybillno.Visible = true;
                txtEwayBillNo.Visible = true;
                radbtnEwayBillDate.Visible = true;
            }
            else if (intIsBillNo == 1 && intIsEwayBillNo == 0)
            {
                lblBillNo.Visible = true;
                txtBillNo.Visible = true;
                dtBillNo.Visible = true;

                lblewaybillno.Visible = false;
                txtEwayBillNo.Visible = false;
                radbtnEwayBillDate.Visible = false;
            }
            else if (intIsBillNo == 0 && intIsEwayBillNo == 1)
            {
                string strlblbill = "";

                Common.swapControlsInTable(txtBillNo, txtEwayBillNo);
                Common.swapControlsInTable(dtBillNo, radbtnEwayBillDate);

                ///************ label text change **********/
                strlblbill = lblBillNo.Text;
                lblBillNo.Text = lblewaybillno.Text;
                lblewaybillno.Text = strlblbill;

                lblBillNo.Visible = true;
                txtBillNo.Visible = false;
                dtBillNo.Visible = false;

                lblewaybillno.Visible = false;
                txtEwayBillNo.Visible = true;
                radbtnEwayBillDate.Visible = true;

            }
            else
            {
                lblBillNo.Visible = false;
                txtBillNo.Visible = false;
                dtBillNo.Visible = false;

                lblewaybillno.Visible = false;
                txtEwayBillNo.Visible = false;
                radbtnEwayBillDate.Visible = false;
            }
        }

        private void GetUserDetails()
        {
            try
            {
                DataTable dt;
                dt = Users.GetList(0, 0, "", "", "", "", 0, -1, intCompanyID);

                dt = new DataView(dt, "(UserID = " + intUserID + ")", "", DataViewRowState.CurrentRows).ToTable();

                if (dt != null && dt.Rows.Count > 0)
                {
                    strLaserPrinterName = dt.Rows[0]["LaserPrinter"].ToString();
                    strStickerPrinterName = dt.Rows[0]["StickerPrinter"].ToString();
                }
            }
            catch (Exception ex)
            {
                strLaserPrinterName = "";
                strStickerPrinterName = "";
            }
        }

        private void radbtnEwayBillDate_Click(object sender, EventArgs e)
        {
            try
            {
                if (dtEwayBillStartDate == null)
                    dtEwayBillStartDate = DateTime.Now;
                if (dtEwayBillEndDate == null)
                    dtEwayBillEndDate = DateTime.Now;

                Cargo.frmEwayBillDate frm = new frmEwayBillDate(this, dtEwayBillStartDate, dtEwayBillEndDate);
                frm.Show();
            }
            catch (Exception ex)
            { }
        }

        public void GetEwayBillDateDetails(DateTime _dtEWayBillStartDate, DateTime _dtEwayBillEndDate)
        {
            try
            {
                dtEwayBillStartDate = _dtEWayBillStartDate;
                dtEwayBillEndDate = _dtEwayBillEndDate;
            }
            catch (Exception ex)
            { }
        }

        private void ResetEwayBillDates()
        {
            try
            {
                
                dtEwayBillStartDate = Common.GetServerTime(intUserID, intCompanyID);
                dtEwayBillEndDate = Common.GetServerTime(intUserID, intCompanyID);
            }
            catch (Exception ex)
            { }
        }

        private void txtEwayBillNo_Leave(object sender, EventArgs e)
        {
            try
            {
                if (txtEwayBillNo.Text.Length < 1)
                {
                    ResetEwayBillDates();
                }
            }
            catch (Exception ex)
            { }
        }
    }
}
