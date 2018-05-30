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
using System.Linq;
using System.IO;

namespace CRS2011DeskApp.Cargo
{
    public partial class frmLuggageBookingV2 : Telerik.WinControls.UI.RadForm, ICargoBooking
    {
        //bool is_hans_config, is_shatabdi_config;
        bool is_insurance_company, is_direct_freight_company, is_crossing_company, is_stax_company;
        bool IsPartyForAllPayType = true;
        bool IsPartyForSenderReceiver = true;

        //string[] Cover_ConsignmentID;
        //string[] Packet_ConsignmentID;
        //string[] Parcel_ConsignmentID;

        private int xPos = 0;
        private int xPos1 = 0;
        private int xPos2 = 0;
        public int intCompanyID = 0;
        public int intBranchID = 0;
        public int intUserID = 0;
        ArrayList arrListCntrl;
        public DataSet dsRights = null;
        public Boolean AllowRateChange = true;
        public Boolean IsClicked = false;
        public Boolean toGetTransfer = false;
        public Boolean toGetShort = false;
        bool blnIsFirstLoad = true;

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
        public int intHasDeliveryType = 0;

        public DataTable dtConsignType = null;
        public DataTable dtGriSourse = null;

        public DataTable dtMOP = null;

        public bool blnIsSTACargoCompany = false;

        public bool blnIsOfflineMode = false;

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
        public Color objColorOffline = Color.FromArgb(216, 191, 216);

        public DataTable dtRateMaster = null;
        public DataTable dtPartyRateMaster = null;
        int celIndex = 0;
        int rowIndex = 0;

        Boolean ToLoad = false;
        Boolean IsValueChanged = false;
        int intIsManual = 0;
        int intIsOnAccount = 0;
        int TotalHamali = 0;
        int TotalInsurance = 0;
        public DataTable dtFromCities = null;
        public DataTable dtToCities = null;
        public DataTable dtExCities = null;
        public string strSelectedLRNo = "";
        bool AllowOfflineBooking = false;
        public int is_GSTN_branch = 0;
        int HasShortReceiptMarquee = 0;
        int HasBranchTransferMarquee = 0;
        int GSTType = 1; //1 -> STA, 2 -> Hans 
        int intReceiveType = 2; // 1 Vehicle & Challan, 2 Challan No

        DataSet dsVehicleNo;

        int intIDProofTypeID = 0;
        string strIDProofNo = "";
        string strSenderImageName = "";
        string strIDProofImageName = "";
        string strRate = "";
        string strPartyRate = "";

        decimal dcmLength = 0;
        decimal dcmWidth = 0;
        decimal dcmHeight = 0;
        decimal dcmVolumetricWeight = 0;

        public delegate void dlgShowLastLRNO();
        public delegate void dlgUploadImgtos3();
        static List<char> SpecialChars = new List<char> { '~', '^', '|', '&' };

        /*************** FTL related Changes **********************************/
        int _intPickupCityID = 0;
        int _intDropOffCityID = 0;
        string _pickupCityShortName = "";
        string _dropoffCityShortName = "";
        bool HasFTLBooking = true;
        string strFTLTypeConsID = "";
        /*************** FTL related Changes (END) **********************************/
        int intIsBillNo = 0;
        int intIsEWayBillNo = 0;
        DateTime dtEwayBillStartDate;
        DateTime dtEwayBillEndDate;
        string strLaserPrinterName = "";
        string strStickerPrinterName = "";
        public int intisBkgCreditLimit = 0;

        private int HasNoBeforeName = 0;
        private int intHasCartageBreakup = 0;
        private int _TotalCartage = 0;
        private int _PickupCartage = 0;
        private int _CommCartage = 0;
        private int _ReturnCartage = 0;
        private string LogoURL = "";
        DataTable dtPartyFrom = new DataTable();
        DataTable dtPartyTo = new DataTable();

        public frmLuggageBookingV2()
        {
            InitializeComponent();
            intCompanyID = Common.GetCompanyID();
            intBranchID = Common.GetBranchID();
            intUserID = Common.GetUserID();
            intBranchCityID = Common.GetBranchCityID();
            blnIsSTACargoCompany = Common.GetCacheIsSTACargoCompany();

            is_insurance_company = is_direct_freight_company = is_crossing_company = is_stax_company = false;
            
            GetRights();
            GetUserDetails();
            CargoSettingConfig();
            SetCargoLogo();
            SwapNameAndNoLables();
            string strCompanyLikeOrange = App_Code.Cargo.CompanyDisplayLikOrange(intUserID, Common.GetLogID());
            if (strCompanyLikeOrange.Split('^')[0].Contains("||" + intCompanyID + "||"))
                IsOrangeTypeDisplay = true;

            Boolean IsDailyExpense = false;
            if (strCompanyLikeOrange.Split('^')[1].Contains("|" + intCompanyID + "|"))
                IsDailyExpense = true;

            radBDailyExpense.Visible = IsDailyExpense;
            //intCompanyID = 1;
            //intBranchID = 1;
            //intUserID = 1;
            //intBranchCityID = 1;

            this.KeyPreview = true;

            txtCollCartageRemark.ForeColor = SystemColors.GrayText;
            txtCollCartageRemark.Text = "Cartage - Mobile and Comment";


            dtFromCities = GetCargoFromCities();
            dtToCities = GetCargoToCities();
            dtExCities = GetCargoExCities();

            //GetRights();


            FillBookingCity();
            radDDBookingCity.SelectedValue = intBranchCityID.ToString();

            //RemoveValuesFromControls();

            //lblCrossingCity.Visible = false;
            //lblCrossingBranch.Visible = false;
            //radDDCrossingCity.Visible = false;
            //radDDCrossingBranch.Visible = false;

            FillServiceTaxPayer();

            //CargoSettingConfig();
            FillDestCities();

            FillBookingBranch();

            //FillCrossingCities();

            radDDBookingBranch.SelectedValue = intBranchID.ToString();

            //FillPartiesforBookingCity();
            //fillPayType();

            FillModeOfPayment();

            FillDeliveryType();

            FillDestinationBranch();

            fillDates();

            //FillCrossingBranch();


            createDT();
            RemoveValuesFromControls();
            //AddNewRow("0", "", "0", "0", "0", "0");
            fillGrid();

            //fillPayType();

            SetTabOrder();

            GetRateMaster();
            GetPartyRateMaster();

            setPaymentMode();
            //chkCrossing.Visible = false;
            radDDCStaxPaidBy.Visible = false;
            radDDCStaxPaidBy.Enabled = false;
            lblStaxPaidBy.Visible = false;
            chkInsurance.Visible = false;
            chkInsurance.Checked = false;

            if (Common.IsCompanyAdminUser() || Common.GetCompanyID() == 1)
            {
                btnChangeVehicle.Visible = true;
            }

            //if (intCompanyID == 403)
            //{
            //    chkCrossing.Visible = true;
            //}

            if (is_stax_company && is_GSTN_branch == 1)
            {
                radDDCStaxPaidBy.Visible = true;
                lblStaxPaidBy.Visible = true;
                radDDCStaxPaidBy.SelectedValue = 2;
            }
            else
                radDDCStaxPaidBy.SelectedValue = 1;

            //if (is_insurance_company)
            //{
            //    chkInsurance.Visible = true;
            //}
            //if (is_crossing_company)
            //{
            //    chkCrossing.Visible = true;
            //}

            //if (intCompanyID == 11 || intCompanyID == 1247 || intCompanyID == 1 || intCompanyID == 1008 || intCompanyID == 977 || intCompanyID == 225 || intCompanyID == 1427 || intCompanyID == 422 || intCompanyID == 1186 || intCompanyID == 227 || intCompanyID == 410 || intCompanyID == 512 || intCompanyID == 1387 || intCompanyID == 1273)
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
            radGridConsignItems.Columns["Freight"].ReadOnly = false;

            chkSenderMobileGetData.Enabled = true;
            chkRecMobileGetData.Enabled = true;


            FetchPartyRateExpire();

            if (blnIsSTACargoCompany)
            {
                UpdateCreditLimit();
            }

            LoadVehicles();
            SetRights();

            #region "MarqueeSection"

            DataTable dtComp = Companies.CompaniesListAll(Common.GetCompanyID(), 0, 0, 1);
            DataSet dsCargoSettings = new DataSet();

            dsCargoSettings = App_Code.Cargo.GetCargoCompanySettings(Common.GetCompanyID(), Common.GetUserID());


            if (dsCargoSettings != null && dsCargoSettings.Tables.Count > 0 && dsCargoSettings.Tables[0].Rows.Count > 0)
            {
                if (dsCargoSettings.Tables[0].Rows[0]["HasBranchTransferMarquee"].ToString() == "1")
                    HasBranchTransferMarquee = 1;
                else
                    HasBranchTransferMarquee = 0;

                if (dsCargoSettings.Tables[0].Rows[0]["HasShortReceivedMarquee"].ToString() == "1")
                    HasShortReceiptMarquee = 1;
                else
                    HasShortReceiptMarquee = 0;
            }


            if (dtComp != null && dtComp.Rows.Count > 0)
            {
                lblOperatorSetMarquee.Text = Convert.ToString(dtComp.Rows[0]["CargoMarquee"]);
            }
            else
            {
                lblOperatorSetMarquee.Text = "";
            }
            #endregion

            SetVisibility();
        }

        private void SetCargoLogo()
        {
            try
            {
                if (LogoURL.Trim() != "")
                {
                    pbCargoLogo.ImageLocation = LogoURL;
                    pbCargoLogo.SizeMode = PictureBoxSizeMode.StretchImage;
                }
                else
                {
                    pbCargoLogo.ImageLocation = @"http://buscrs.com/content/logo/DefaultCompanyLogo.jpg";
                    pbCargoLogo.SizeMode = PictureBoxSizeMode.StretchImage;
                }
            }
            catch (Exception ex)
            { }
        }

        private bool IsStaxCompany()
        {
            try
            {
                DataSet dsSettings = App_Code.Cargo.GetCargoCompanySettings(intCompanyID, intUserID);

                int HasServiceTax = Convert.ToInt32(dsSettings.Tables[0].Rows[0]["IsServiceTax"]);
                if (HasServiceTax == 0)
                    return false;
                else
                    return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void SetRights()
        {
            try
            {
                DataTable dstOfflineBooking = new DataView(dsRights.Tables[0], "BookingRightsID=11", "", DataViewRowState.CurrentRows).ToTable();
                Boolean blnIsOfflineBooking = Convert.ToBoolean(dstOfflineBooking.Rows[0]["Allowed"]);

                if (blnIsOfflineBooking || Common.IsMTPLAdminUser() || Common.IsCompanyAdminUser())
                    radBOfflineBooking.Enabled = true;
            }
            catch (Exception ex) { }
        }

        private void SetVisibility()
        {
            //int companyid = Common.GetCompanyID();

            if (intCompanyID == 2696 || intCompanyID == 180 || intCompanyID == 1298) // Companies:= Green Pacel Services:2696,MRTravels, 
            {

                //txtCartageDel.Text = "0";
                //txtCollCartageRemark.Text = "";

                label17.Visible = false;
                radDModeofPayment.Visible = false;

                //lblCartageDel.Visible = false;
                //txtCartageDel.Visible = false;

                //label2.Visible = false;
                //txtCollCartageRemark.Visible = false;

                //lblHamaliChrg.Visible = true;
                //txtHamaliChg.Text = "0";
                //txtHamaliChg.Visible = true;

                if (intCompanyID == 180 || intCompanyID == 1298)
                {
                    txtSenderEmailID.Text = "";
                    txtSenderEmailID.Enabled = false;
                    txtReceiverEmailID.Text = "";
                    txtReceiverEmailID.Enabled = false;
                }
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

        public void SetDocumentCharge(int intPaymentType)
        {
            try
            {
                txtDocumentChg.Text = "0";
                int IsConsignmentSelected = 1;

                if (!blnIsOfflineMode && txtDocumentChg.Visible == true)
                {
                    if (Convert.ToInt32(txtFreightChg.Text) <= 60 && (intCompanyID == 66 || intCompanyID == 184 || intCompanyID == 805 || intCompanyID == 2649 || intCompanyID == 2650 || intCompanyID == 322))
                    {
                        txtDocumentChg.Text = "0";
                        txtDocumentChg.Enabled = false;
                    }
                    else
                    {
                        txtDocumentChg.Enabled = true;

                        DataSet dstDocumentCharge = null;

                        dstDocumentCharge = App_Code.Cargo.CargoDocumentChargesListAll(intCompanyID);

                        for (int i = 0; i < radGridConsignItems.Rows.Count; i++)
                        {
                            string ConsignmentSubType = radGridConsignItems.Rows[i].Cells["ConsignmentType"].Value.ToString();
                            string ConsignMentTypeID = ConsignmentSubType.Split('-')[0].Trim();

                            if (ConsignMentTypeID == "0")
                                continue;

                            if (IsConsignmentSelected == 1)
                            {
                                DataTable dtConsignmentSettings = new DataView(dtConsignType, "ConsignmentSubTypeID1 = " + ConsignMentTypeID,
                                                                                               "", DataViewRowState.CurrentRows).ToTable();

                                int BillingUnit = Convert.ToInt32(dtConsignmentSettings.Rows[0]["BillingUnit"].ToString());

                                DataTable dtLrWiseDocCharge = null;
                                DataTable dtUnitWiseDocCharge = null;

                                try
                                {
                                    dtLrWiseDocCharge = new DataView(dstDocumentCharge.Tables[0], "ChargeTypeID = 1 and " + "BillingUnit = " + BillingUnit + " and PayTypeID=" + intPaymentType, "", DataViewRowState.CurrentRows).ToTable();
                                    dtUnitWiseDocCharge = new DataView(dstDocumentCharge.Tables[0], "ChargeTypeID = 2 and " + "BillingUnit = " + BillingUnit + " and PayTypeID=" + intPaymentType, "", DataViewRowState.CurrentRows).ToTable();
                                }
                                catch (Exception ex)
                                {
                                    dtLrWiseDocCharge = null;
                                    dtUnitWiseDocCharge = null;
                                }


                                if (dtLrWiseDocCharge.Rows.Count > 0)
                                {
                                    try
                                    {
                                        txtDocumentChg.Text = (Convert.ToInt32(Convert.ToDecimal(txtDocumentChg.Text) + Convert.ToDecimal(dtLrWiseDocCharge.Rows[0]["DocumentCharge"]))).ToString();
                                    }
                                    catch (Exception ex)
                                    { }
                                }
                                else if (dtUnitWiseDocCharge != null && dtUnitWiseDocCharge.Rows.Count > 0)
                                {
                                    try
                                    {
                                        decimal Tot_Unit = Convert.ToDecimal(radGridConsignItems.Rows[i].Cells["Qty"].Value);
                                        txtDocumentChg.Text = (Convert.ToInt32(Convert.ToDecimal(dtUnitWiseDocCharge.Rows[0]["DocumentCharge"]) * Tot_Unit)).ToString();
                                    }
                                    catch (Exception ex)
                                    { }
                                }
                                else
                                {
                                    txtDocumentChg.Text = (Convert.ToDecimal(txtDocumentChg.Text) + 0).ToString();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                txtDocumentChg.Text = "0";
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

        public void GetPartyRateMaster()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                if (intBranchCityID == 0)
                    return;

                DataSet dsPartyRateMaster = App_Code.Cargo.GetRateMaster(intCompanyID, intBranchCityID, 0, 0, intBranchID, intUserID, 2, "Party", 0);

                if (dsPartyRateMaster != null && dsPartyRateMaster.Tables.Count > 0 && dsPartyRateMaster.Tables[0].Rows.Count > 0)
                {
                    dtPartyRateMaster = dsPartyRateMaster.Tables[0].Copy();
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

        public frmLuggageBookingV2(Boolean _ToLoad)
            : this()
        {
            ToLoad = _ToLoad;
        }

        private void SetTabOrder()
        {
            try
            {
                arrListCntrl = new ArrayList();
                arrListCntrl.Add(radDDBookingCity);
                arrListCntrl.Add(radDDBookingBranch);
                arrListCntrl.Add(radDDDestCity);
                arrListCntrl.Add(radDDDestBranch);
                arrListCntrl.Add(chkFTLAssignCities);
                arrListCntrl.Add(raddDeliveryType);
                arrListCntrl.Add(radDPayType);
                arrListCntrl.Add(txtManualLR);
                arrListCntrl.Add(chkIsPartySender);
                arrListCntrl.Add(txtSearchPartySender);
                arrListCntrl.Add(radDPartySender);

                if (HasNoBeforeName == 1)
                {
                    arrListCntrl.Add(txtMobileNo);
                    arrListCntrl.Add(txtNameSender);
                }
                else
                {
                    arrListCntrl.Add(txtNameSender);
                    arrListCntrl.Add(txtMobileNo);
                }
                arrListCntrl.Add(txtSenderAddress);
                arrListCntrl.Add(txtSenderEmailID);
                arrListCntrl.Add(chkIsCollection);
                arrListCntrl.Add(radDCollectionType);
                arrListCntrl.Add(txtBillNo);
                //arrListCntrl.Add(radDDDDBill);
                //arrListCntrl.Add(radDDMMBill);
                //arrListCntrl.Add(radDDYYBill);
                arrListCntrl.Add(dtBillNo);
                arrListCntrl.Add(txtEwayBillNo);
                arrListCntrl.Add(radbtnEwayBillDate);
                //arrListCntrl.Add(radDDCStaxPaidBy);
                arrListCntrl.Add(txtSenderGSTN);

                arrListCntrl.Add(chkIsPartyReceiver);
                arrListCntrl.Add(txtSearchPartyReceiver);
                arrListCntrl.Add(radDPartyReceiver);

                if (HasNoBeforeName == 1)
                {
                    arrListCntrl.Add(txtMobileNoReceiver);
                    arrListCntrl.Add(txtNameReceiver);
                }
                else
                {
                    arrListCntrl.Add(txtNameReceiver);
                    arrListCntrl.Add(txtMobileNoReceiver);
                }

                arrListCntrl.Add(txtAddressReceiver);
                arrListCntrl.Add(txtReceiverEmailID);
                arrListCntrl.Add(txtReceiverGSTN);
                arrListCntrl.Add(chkInsurance);

                arrListCntrl.Add(radGridConsignItems);

                arrListCntrl.Add(txtCartage);
                arrListCntrl.Add(txtCartageDel);

                arrListCntrl.Add(radDVehicleNos);
                arrListCntrl.Add(txtBusNoSearch1);
                arrListCntrl.Add(txtHamaliChg);
                arrListCntrl.Add(txtInsurance);
                arrListCntrl.Add(txtDocumentChg);
                arrListCntrl.Add(txtServiceTax);

                arrListCntrl.Add(txtDeliveryChg);

                arrListCntrl.Add(txtCollCartageRemark);

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

                //DataTable dtPartyFrom = new DataTable();

                DataTable dtParty = Common.GetLuggageParties("B");

                if (dtParty != null && dtParty.Rows.Count > 0)
                {
                    string strQuery = "CityID=" + radDDBookingCity.SelectedValue.ToString();

                    //if (intCompanyID == 1 || intCompanyID == 406)
                    //{
                    //    if (radDPayType.SelectedValue.ToString() == "5") // On Account
                    //        strQuery += " and IsBookingCreditLimit=1";
                    //}

                    dtPartyFrom = new DataView(dtParty, strQuery, "", DataViewRowState.CurrentRows).ToTable();

                    //dtPartyFrom = new DataView(dtParty, "CityID=" + radDDBookingCity.SelectedValue.ToString(), "", DataViewRowState.CurrentRows).ToTable();

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
                //DataTable dtPartyTo = new DataTable();

                DataTable dtParty = Common.GetLuggageParties("B");

                if (dtParty != null && dtParty.Rows.Count > 0)
                {
                    //Pradeep : 2017-12-01 : commenting as party can have multiple cities change is going live
                    //if (blnIsSTACargoCompany && intCompanyID == 406)
                    //    dtPartyTo = new DataView(dtParty, "PartyCitiesIDs=" + radDDBookingCity.SelectedValue.ToString(), "", DataViewRowState.CurrentRows).ToTable();
                    //else
                    dtPartyTo = new DataView(dtParty, "CityID=" + radDDDestCity.SelectedValue.ToString(), "", DataViewRowState.CurrentRows).ToTable();

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

        private void frmLuggageBookingV2_Load(object sender, EventArgs e)
        {
            if (ToLoad)
                tableLayoutPanel1.Visible = true;

            //string marqeeAllowed = App_Code.Cargo.GetMarqeeAllowedCompanyID(intUserID);
            //string strCompID = "||" + intCompanyID + "||";

            //if (marqeeAllowed.Contains(strCompID))
            //{
            xPos = panel1.Width;

            if (HasShortReceiptMarquee == 1 && HasBranchTransferMarquee == 1)
            {
                GetAlerts(3);
            }
            else if (HasBranchTransferMarquee == 1)
            {
                GetAlerts(1);
            }
            else if (HasShortReceiptMarquee == 1)
            {
                GetAlerts(2);
            }
            //GetTransferAlerts();
            //GetShortReceiptAlerts();
            //timer1.Start();
            //}

            txtSenderAddress.Text = radDDBookingCity.Text;
            //xPos1 = panel1.Width;

            //timer2.Start();
        }

        public void createDT()
        {
            try
            {
                dtConsignType = new DataTable();

                dtConsignType = Common.GetLuggageConsignTypes("B");

                DataRow dr = null;

                if (dtConsignType != null && dtConsignType.Rows.Count > 0)
                {
                    dr = dtConsignType.NewRow();
                    dr["SubType"] = "--Select--";
                    dr["ConsignmentSubTypeID"] = "0";
                    dtConsignType.Rows.InsertAt(dr, 0);

                    DataRow[] drw = dtConsignType.Select("SubType = 'FTL'");
                    if (drw.Length > 0)
                        strFTLTypeConsID = drw[0]["ConsignmentSubTypeID"].ToString();
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

                dtMOP = new DataTable();

                dtMOP = App_Code.Cargo.GetMethodOfPacking(intCompanyID, intUserID, 1);


                DataRow drMOP = null;

                if (dtMOP != null && dtMOP.Rows.Count > 0)
                {
                    drMOP = dtMOP.NewRow();
                    drMOP["MOPName"] = "--Select--";
                    drMOP["MOPID"] = "0";
                    dtMOP.Rows.InsertAt(drMOP, 0);
                }

                dtGriSourse = new DataTable();
                DataColumn dc = new DataColumn("ConsignmentTypeID");
                dc.DataType = System.Type.GetType("System.String");
                dtGriSourse.Columns.Add(dc);

                dc = new DataColumn("MOPID");
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

                dc = new DataColumn("WeightChrg");
                dc.DataType = System.Type.GetType("System.Double");
                dtGriSourse.Columns.Add(dc);

                dc = new DataColumn("Rate");
                dc.DataType = System.Type.GetType("System.String");
                dtGriSourse.Columns.Add(dc);

                dc = new DataColumn("Freight");
                dc.DataType = System.Type.GetType("System.Double");
                dtGriSourse.Columns.Add(dc);

                dc = new DataColumn("IsVolumetricWeight");
                dc.DataType = System.Type.GetType("System.Int32");
                dtGriSourse.Columns.Add(dc);

                dc = new DataColumn("Length");
                dc.DataType = System.Type.GetType("System.Int32");
                dtGriSourse.Columns.Add(dc);

                dc = new DataColumn("Width");
                dc.DataType = System.Type.GetType("System.Int32");
                dtGriSourse.Columns.Add(dc);

                dc = new DataColumn("Height");
                dc.DataType = System.Type.GetType("System.Int32");
                dtGriSourse.Columns.Add(dc);
            }
            catch (Exception ex)
            { }
        }

        public void AddNewRow(string ConsignmentType, string MOP, string Description, string Qty, string GoodsValue, string Rate, string Freight, double Weight, double WeightChrg,
                double IsVolumetricWeight, double Length, double Width, double Height)
        {
            radGridConsignItems.DataSource = null;

            DataRow drw = dtGriSourse.NewRow();
            drw["ConsignmentTypeID"] = ConsignmentType;
            drw["MOPID"] = MOP;
            drw["Description"] = Description;
            drw["Qty"] = Qty;
            drw["Goodsvalue"] = GoodsValue;
            drw["Weight"] = Weight;
            drw["WeightChrg"] = WeightChrg;
            drw["Rate"] = Rate;
            drw["Freight"] = Freight;
            drw["IsVolumetricWeight"] = IsVolumetricWeight;
            drw["Length"] = Length;
            drw["Width"] = Width;
            drw["Height"] = Height;
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


                ((Telerik.WinControls.UI.GridViewComboBoxColumn)radGridConsignItems.Columns["MOP"]).DataSource = dtMOP;
                ((Telerik.WinControls.UI.GridViewComboBoxColumn)radGridConsignItems.Columns["MOP"]).DisplayMember = "MOPName";
                ((Telerik.WinControls.UI.GridViewComboBoxColumn)radGridConsignItems.Columns["MOP"]).ValueMember = "MOPID";
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

        public void FillBookingCity(bool isOffline = false)
        {
            try
            {

                DataSet ds = new DataSet();
                DataTable dtFrmCities = new DataTable();

                //dtFrmCities = Common.GetCompanyCities("B");
                dtFrmCities = App_Code.Cargo.CargoCompanyCitiesListAll(intUserID, intCompanyID, 0);

                DataView dv;
                DataTable dtFrmCts;

                if (isOffline)
                {
                    dv = new DataView(dtFrmCities, "CityID<>" + intBranchCityID, "CityName ASC", DataViewRowState.CurrentRows);
                    dtFrmCts = dv.ToTable();
                }
                else
                {
                    dv = new DataView(dtFrmCities, "", "CityName ASC", DataViewRowState.CurrentRows);

                    dtFrmCts = dv.ToTable();

                    int intActualCity = new DataView(dtFrmCts, "CityID=" + intBranchCityID, "", DataViewRowState.CurrentRows).ToTable().Rows.Count;

                    if (intActualCity <= 0)
                        dtFrmCts = null;
                }

                radDDBookingCity.DisplayMember = "CityName";
                radDDBookingCity.ValueMember = "CityID";
                radDDBookingCity.DataSource = dtFrmCts;

            }
            catch (Exception ex)
            {

            }
        }

        public void FillDestCities(bool isOffline = false)
        {
            try
            {

                DataSet ds = new DataSet();
                DataTable dtDestCities = new DataTable();

                //dtDestCities = Common.GetCompanyCities("B");
                dtDestCities = App_Code.Cargo.CargoCompanyCitiesListAll(intUserID, intCompanyID, 0);

                dtDestCities = new DataView(dtDestCities, "CitiHasCargoDelivery=1", "CityName ASC", DataViewRowState.CurrentRows).ToTable();

                DataTable dtToCts;
                if (isOffline)
                    dtToCts = dtDestCities;
                else
                    dtToCts = new DataView(dtDestCities, "CityID <>" + intBranchCityID.ToString(), "CityName ASC", DataViewRowState.CurrentRows).ToTable();

                List<DataRow> rowsToDelete = new List<DataRow>();
                foreach (DataRow dr in dtToCts.Rows)
                {
                    DataTable dtTemp = new DataView(dtToCities, "CityID=" + dr["CityID"] + "AND IsPaid = 0 AND IsToPay = 0 AND IsFoc = 0 AND IsCOD = 0 AND OnAcc = 0", "CityName ASC", DataViewRowState.CurrentRows).ToTable();

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

                try
                {
                    if (isOffline)
                    {
                        if (intCompanyID == 805 || intCompanyID == 2921)
                            radDDDestCity.SelectedValue = "38";
                        radDDDestCity.Enabled = false;
                    }
                }
                catch (Exception ex)
                { }

            }
            catch (Exception ex)
            {

            }
        }

        //public void FillCrossingCities()
        //{
        //    DataTable dtDestCities = new DataTable();

        //    dtDestCities = Common.GetCompanyCities("B");
        //    try
        //    {
        //        DataTable dtToCts = new DataView(dtDestCities, "CityID <>" + intBranchCityID.ToString() + " and CityID <> " + radDDDestCity.SelectedValue.ToString(), "CityName ASC", DataViewRowState.CurrentRows).ToTable();

        //        List<DataRow> rowsToDelete = new List<DataRow>();
        //        foreach (DataRow dr in dtToCts.Rows)
        //        {
        //            DataTable dtTemp = new DataView(dtExCities, "CityID=" + dr["CityID"] + " AND IsPaid = 0 AND IsToPay = 0 AND IsFoc = 0 AND IsCOD = 0 AND OnAcc = 0", "", DataViewRowState.CurrentRows).ToTable();

        //            if (dtTemp != null && dtTemp.Rows.Count > 0)
        //            {
        //                rowsToDelete.Add(dr);
        //            }
        //        }

        //        foreach (var dataRow in rowsToDelete)
        //        {
        //            dtToCts.Rows.Remove(dataRow);
        //        }

        //        radDDCrossingCity.DisplayMember = "CityName";
        //        radDDCrossingCity.ValueMember = "CityID";
        //        radDDCrossingCity.DataSource = dtToCts;

        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}

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

        public void FillBookingBranch(bool LoadOffline = false)
        {
            try
            {

                DataSet ds = new DataSet();
                DataTable dtBranch = new DataTable();
                DataTable dtTempBranch = new DataTable();
                DataTable dtLuggageBranch = new DataTable();

                radDDBookingBranch.DataSource = null;

                //dtBranch = Common.GetBranches("B");
                dtBranch = Common.GetBranchesCargoShared("B", Convert.ToInt32("0" + radDDBookingCity.SelectedValue.ToString()),0);

                if (!LoadOffline && dtBranch != null && dtBranch.Rows.Count > 0)
                {
                    DataTable dtCurBranch = new DataTable();
                    DataTable dtTBranch = dtBranch.Copy();


                    dtCurBranch = new DataView(dtTBranch, "BranchID = " + intBranchID, "BranchName ASC", DataViewRowState.CurrentRows).ToTable();

                    is_GSTN_branch = Convert.ToInt16(dtCurBranch.Rows[0]["CargoHasSTax"]);
                }

                dtTempBranch = dtBranch.Copy();

                if (LoadOffline)
                {
                    DataView dvTemp = dtTempBranch.DefaultView;
                    dvTemp.RowFilter = " BranchID <> " + intBranchID + " and CityID = " + radDDBookingCity.SelectedValue + " and CargoHasDeliveryManual = 1";
                    dtLuggageBranch = dvTemp.ToTable();
                    //return dt;

                    //dtLuggageBranch = new DataView(dtTempBranch, "BranchTypeID in ('2','3') and BranchID <> " + intBranchID + " and CityID = " + radDDBookingCity.SelectedValue + " and CargoHasDeliveryManual = 1 ", "BranchName ASC", DataViewRowState.CurrentRows).ToTable();
                }
                else
                {
                    dtLuggageBranch = new DataView(dtTempBranch, "BranchTypeID in ('2','3')", "BranchName ASC", DataViewRowState.CurrentRows).ToTable();
                }

                if (!LoadOffline)
                {
                    int intBkgBrnchActual = new DataView(dtLuggageBranch, "BranchID=" + Common.GetBranchID(), "", DataViewRowState.CurrentRows).ToTable().Rows.Count;

                    if (intBkgBrnchActual <= 0)
                        dtLuggageBranch = null;
                }

                radDDBookingBranch.DisplayMember = "BranchName";
                radDDBookingBranch.ValueMember = "BranchID";
                radDDBookingBranch.DataSource = dtLuggageBranch;

            }
            catch (Exception ex)
            {

            }
        }

        public void UpdateCreditLimit()
        {
            string strErrMsg = "";
            if (blnIsSTACargoCompany)
            {
                DataTable dt = App_Code.Cargo.CargoBranchCreditLimit(intCompanyID, intBranchID, ref strErrMsg);

                if (dt != null && dt.Rows.Count > 0)
                {
                    int IsCargoCreditLimit = Convert.ToInt32(dt.Rows[0]["IsCargoCreditLimit"]);
                    decimal CargoBalance = Convert.ToDecimal(dt.Rows[0]["CargoBalance"]);
                    int CargoCreditLimit = Convert.ToInt32(dt.Rows[0]["CargoCreditLimit"]);
                    int CargoHasDeliveryManual = Convert.ToInt32(dt.Rows[0]["CargoHasDeliveryManual"]);

                    //if (CargoHasDeliveryManual == 0)
                    //{
                    //    radBOfflineBooking.Visible = false;
                    //}
                    //else
                    //{
                    //    radBOfflineBooking.Visible = true;
                    //}

                    if (IsCargoCreditLimit == 1)
                    {
                        string strBalance = "";

                        if (CargoCreditLimit != -1)
                        {
                            decimal dcmUsagePct = (CargoBalance * 100) / CargoCreditLimit;

                            strBalance = "Credit Limit : " + CargoCreditLimit + ", Usage : " + CargoBalance + ", " + dcmUsagePct.ToString("0.##") + "%";

                            if (dcmUsagePct >= 75)
                                lblCreditValues.BackColor = Color.Blue;
                            else
                                lblCreditValues.BackColor = Color.White;
                        }
                        else
                            strBalance = "Credit Limit : Unlimited, Usage : " + CargoBalance;

                        lblCreditValues.Text = strBalance;

                        radbtnRequestRecharge.Visible = true;

                        if (Common.IsBranchAdminUser() || Common.IsMTPLAdminUser() || Common.IsCompanyAdminUser())
                            radbtnUpdateRecharge.Visible = true;

                    }
                    else
                        lblCreditValues.Text = "";
                }

                //radDDDDBill.Visible = false;
                //radDDMMBill.Visible = false;
                //radDDYYBill.Visible = false;
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

        public void FetchPartyRateExpire()
        {
            try
            {
                DataSet ds = new DataSet();

                ds = App_Code.Cargo.FetchPartyRateExpire(intUserID, intCompanyID, intBranchCityID);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    this.Cursor = Cursors.WaitCursor;
                    //tableLayoutPanel1.Enabled = false;
                    //tableLayoutPanel1.BackColor = Color.Silver;

                    Cargo.frmPartyRateExpire frm = new Cargo.frmPartyRateExpire(ds);
                    frm.ShowDialog();
                    this.Cursor = Cursors.Default;

                    //tableLayoutPanel1.BackColor = Color.White;
                    //tableLayoutPanel1.Enabled = true;
                }

            }
            catch (Exception ex)
            {
            }
        }

        //private void FillCrossingBranch()
        //{
        //    try
        //    {
        //        DataTable dtBranch = new DataTable();
        //        radDDCrossingBranch.DataSource = null;
        //        //dtBranch = Common.GetBranches("B");
        //        dtBranch = Common.GetBranchesCargoShared("B", Convert.ToInt32("0" + radDDCrossingCity.SelectedValue.ToString()));

        //        DataTable dtTempBranch1 = dtBranch.Copy();

        //        DataTable dtDestBranches = new DataView(dtTempBranch1, "BranchTypeID in ('2','3') AND CityID=" + radDDCrossingCity.SelectedValue.ToString(), "BranchName asc", DataViewRowState.CurrentRows).ToTable();

        //        radDDCrossingBranch.DisplayMember = "BranchName";
        //        radDDCrossingBranch.ValueMember = "BranchID";
        //        radDDCrossingBranch.DataSource = dtDestBranches;
        //    }
        //    catch (Exception)
        //    {
        //    }
        //}

        private void fillDates()
        {
            List<clsMonth> objMonth = new List<clsMonth>();
            //List<clsMonth> objMonth2 = new List<clsMonth>();

            System.Globalization.DateTimeFormatInfo mfi = new System.Globalization.DateTimeFormatInfo();

            for (int i = 1; i <= 31; i++)
            {
                radDDDD.Items.Add(i.ToString());
                //radDDDDBill.Items.Add(i.ToString());
            }
            for (int i = 1; i <= 12; i++)
            {
                objMonth.Add(new clsMonth(i.ToString(), mfi.GetAbbreviatedMonthName(i)));
                //objMonth2.Add(new clsMonth(i.ToString(), mfi.GetAbbreviatedMonthName(i)));
            }
            radDDMM.DisplayMember = "MonthName";
            radDDMM.ValueMember = "MonthID";
            radDDMM.DataSource = objMonth;

            //radDDMMBill.DisplayMember = "MonthName";
            //radDDMMBill.ValueMember = "MonthID";
            //radDDMMBill.DataSource = objMonth;

            int intYear = DateTime.Now.Year;
            radDDYY.Items.Add((intYear - 1).ToString());
            radDDYY.Items.Add(intYear.ToString());
            radDDYY.Items.Add((intYear + 1).ToString());

            //radDDYYBill.Items.Add((intYear - 1).ToString());
            //radDDYYBill.Items.Add(intYear.ToString());
            //radDDYYBill.Items.Add((intYear + 1).ToString());

            radDDDD.Text = DateTime.Now.Day.ToString();
            radDDMM.SelectedValue = DateTime.Now.Month.ToString();
            radDDYY.Text = DateTime.Now.Year.ToString();

            //radDDDDBill.Text = DateTime.Now.Day.ToString();
            //radDDMMBill.SelectedValue = DateTime.Now.Month.ToString();
            //radDDYYBill.Text = DateTime.Now.Year.ToString();
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

        public void FillDeliveryType()
        {
            try
            {

                DataTable dtDelType = new DataTable();

                DataColumn dc = new DataColumn("DeliveryType");
                dc.DataType = System.Type.GetType("System.String");
                dtDelType.Columns.Add(dc);

                dc = new DataColumn("DeliveryTypeID");
                dc.DataType = System.Type.GetType("System.String");
                dtDelType.Columns.Add(dc);

                DataRow dr = dtDelType.NewRow();
                dr["DeliveryType"] = "Godown";
                dr["DeliveryTypeID"] = "1";
                dtDelType.Rows.Add(dr);

                dr = dtDelType.NewRow();
                dr["DeliveryType"] = "Home";
                dr["DeliveryTypeID"] = "2";
                dtDelType.Rows.Add(dr);

                raddDeliveryType.DisplayMember = "DeliveryType";
                raddDeliveryType.ValueMember = "DeliveryTypeID";
                raddDeliveryType.DataSource = dtDelType;

                raddDeliveryType.SelectedIndex = 0;

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

                dsCargoTax = App_Code.Cargo.GetCargoCompanyTax(intCompanyID, intBranchID);
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
                    if (dsCorgoSettings.Tables[0].Rows[0]["IsParty"].ToString() == "0")
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

                    if (dsCorgoSettings.Tables[0].Rows[0]["IsPartyForAllPayType"].ToString() == "0") IsPartyForAllPayType = false;

                    if (dsCorgoSettings.Tables[0].Rows[0]["IsPartyForSenderReceiver"].ToString() == "0") IsPartyForSenderReceiver = false;

                    #endregion

                    int intIsFreight = 0, intIsCollChg = 0, intIsCartage = 0,
                        intIsDocument = 0, intIsInsurance = 0, intIsServiceTax = 0;

                    #region "OnAccountAndManual"
                    intIsManual = Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["IsManual"]);
                    intIsOnAccount = Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["IsOnAccount"]);
                    #endregion

                    #region "BillNo"
                    intIsBillNo = Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["IsBillNo"]);
                    intIsEWayBillNo = Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["HasEwayBillNo"]);

                    SetVisibilityOfBillAndEwayBillNo(intIsBillNo, intIsEWayBillNo);
                    #endregion

                    #region "CollChg"
                    intIsCollChg = Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["IsCollChg"]);

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

                    //if (is_insurance_company)
                    //{
                    //    lblInsuranceBx.Visible = true;
                    //    txtInsurance.Visible = true;
                    //    txtInsurance.Enabled = false;
                    //}
                    if (intCompanyID == 406)
                        txtInsurance.Enabled = false;
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
                        radGridConsignItems.Columns[2].IsVisible = false;

                    #endregion

                    #region "GoodsValue"
                    int intIsGoodsValue = Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["IsGoodsValue"]);

                    if (intIsGoodsValue == 0)
                        radGridConsignItems.Columns[4].IsVisible = false;
                    #endregion

                    #region "Weight"
                    int intIsWeight = Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["IsWeight"]);

                    if (intIsWeight == 0)
                    {
                        radGridConsignItems.Columns[5].IsVisible = false;
                        radGridConsignItems.Columns[6].IsVisible = false;
                    }

                    if (intCompanyID == 438)
                    {
                        txtComment.Text = ".";
                        txtComment.Visible = false;
                    }
                    #endregion

                    #region "Hamali"
                    int intIsHamali = Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["IsHamali"]);
                    if (intIsHamali == 0)
                    {
                        txtHamaliChg.Text = "0";
                        lblHamaliChrg.Visible = false;
                        txtHamaliChg.Visible = false;
                    }
                    else
                    {
                        lblHamaliChrg.Visible = true;
                        txtHamaliChg.Visible = true;
                    }
                    #endregion

                    #region "Delivery Cartage"
                    int intIsDelCartage = Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["IsDelCartage"]);
                    if (intIsDelCartage == 0)
                    {
                        txtCartageDel.Text = "0";
                        lblCartageDel.Visible = false;
                        txtCartageDel.Visible = false;
                    }
                    else
                    {
                        lblCartageDel.Visible = true;
                        txtCartageDel.Visible = true;
                    }
                    #endregion

                    #region "Cartage Remark"
                    int intIsCartageRemark = Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["IsCartageRemark"]);
                    if (intIsCartageRemark == 0)
                    {
                        txtCollCartageRemark.Text = "";
                        label2.Visible = false;
                        txtCollCartageRemark.Visible = false;
                    }
                    else
                    {
                        label2.Visible = true;
                        txtCollCartageRemark.Visible = true;
                    }
                    #endregion

                    #region "GSTType"
                    try
                    {
                        GSTType = Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["GSTType"]);
                    }
                    catch (Exception ex)
                    {
                        GSTType = 2;
                    }

                    #endregion

                    #region "ReceiveType"
                    try
                    {
                        intReceiveType = Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["ReceiveType"]);
                    }
                    catch (Exception ex)
                    {
                        intReceiveType = 2;
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

                    #region Delivery Type
                    try
                    {
                        intHasDeliveryType = (Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["IsDelivery"]) == 1 ? 1 : 0);
                    }
                    catch (Exception ex)
                    {
                        intHasDeliveryType = 0;
                    }

                    if (intHasDeliveryType == 1)
                    {
                        lblDeliveryType.Visible = true;
                        raddDeliveryType.Visible = true;
                    }
                    else
                    {
                        lblDeliveryType.Visible = false;
                        raddDeliveryType.Visible = false;
                    }
                    #endregion

                    #region FTL Booking
                    try
                    {
                        HasFTLBooking = (Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["HasFTLBooking"]) == 1 ? true : false);

                    }
                    catch (Exception ex)
                    {
                        HasFTLBooking = false;
                    }

                    if (HasFTLBooking)
                    {
                        lblFTL.Visible = true;
                        chkFTLAssignCities.Checked = false;
                        chkFTLAssignCities.Visible = true;
                        lnkAssignedPickups.Text = "";
                        lnkAssignedPickups.Visible = true;
                    }
                    else
                    {
                        lblFTL.Visible = false;
                        chkFTLAssignCities.Checked = false;
                        chkFTLAssignCities.Visible = false;
                        lnkAssignedPickups.Text = "";
                        lnkAssignedPickups.Visible = false;
                    }

                    #endregion

                    #region CartageBreakup
                    try
                    {
                        intHasCartageBreakup = (Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["HasCartageBreakup"]) == 1 ? 1 : 0);
                    }
                    catch (Exception ex)
                    {
                        intHasCartageBreakup = 0;
                    }

                    #endregion

                    #region SenderNoAtTop
                    try
                    {
                        HasNoBeforeName = (Convert.ToInt32(dsCorgoSettings.Tables[0].Rows[0]["HasNoBeforeName"]) == 1 ? 1 : 0);
                    }
                    catch (Exception ex)
                    {
                        HasNoBeforeName = 0;
                    }

                    #endregion

                    #region Cargo Logo URL
                    try
                    {
                        LogoURL = dsCorgoSettings.Tables[0].Rows[0]["CargoLogoURL"].ToString();
                    }
                    catch (Exception ex)
                    {
                        LogoURL = "";
                    }

                    #endregion
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

        public void SetPartyControls()
        {
            try
            {

                if (IsPartyForAllPayType)
                {
                    lblIsParty.Visible = true;
                    lblIsParty2.Visible = true;

                    if (IsPartyForSenderReceiver)
                    {
                        chkIsPartySender.Enabled = true;
                        //   chkIsPartySender.Checked = true;
                        chkIsPartyReceiver.Enabled = true;
                    }
                    else
                    {
                        chkIsPartySender.Enabled = false;
                        //  chkIsPartySender.Checked = true;
                        chkIsPartyReceiver.Enabled = true;
                    }
                }
                else  // Show Party Only For On Account
                {
                    if (radDPayType.SelectedValue.ToString() == "5") // On Account
                    {
                        if (Convert.ToInt32(dsRights.Tables[1].Rows[0]["AllowPartyBooking"]) == 1)
                        {
                            lblIsParty.Visible = true;
                            lblIsParty2.Visible = true;
                            chkIsPartySender.Visible = true;
                            chkIsPartyReceiver.Visible = true;
                            if (IsPartyForSenderReceiver)
                            {
                                chkIsPartySender.Enabled = true;
                                //    chkIsPartySender.Checked = true;
                                chkIsPartyReceiver.Enabled = true;
                            }
                            else
                            {
                                chkIsPartySender.Enabled = false;
                                chkIsPartySender.Checked = true;
                                chkIsPartyReceiver.Enabled = true;
                            }
                        }
                    }
                    else
                    {
                        chkIsPartySender.Enabled = false;
                        chkIsPartyReceiver.Enabled = false;
                        chkIsPartySender.Checked = false;
                        chkIsPartyReceiver.Checked = false;
                        chkIsPartySender.Visible = false;
                        chkIsPartyReceiver.Visible = false;
                        lblIsParty.Visible = false;
                        lblIsParty2.Visible = false;
                    }
                }

                if (intCompanyID == 406 || intCompanyID == 1)
                {
                    if (radDPayType.SelectedValue.ToString() == "5") // On Account
                    {
                        chkIsPartySender.Checked = true;
                        chkIsPartySender.Enabled = false;
                    }
                    else
                    {
                        chkIsPartySender.Checked = false;
                        chkIsPartySender.Enabled = true;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void tableLayoutPanel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void radDropDownList1_SelectedIndexChanged(object sender, Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
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
                //FillCrossingCities();
                FillPartiesforDestCity();
                fillPayType();

                txtAddressReceiver.Text = radDDDestCity.Text;

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

            if (radGridConsignItems.Rows[rowIndex].Cells["ConsignmentType"].Value.ToString() == "0")
            {
                radGridConsignItems.Rows[rowIndex].Cells["ConsignmentType"].BeginEdit();
                return;
            }

            int intMinAllUnitChargeWeight = 0;
            int intMinPerUnitChargeWeight = 0;
            int intIsMOPEditable = 1;
            int intBillingUnit = 1; // 1 "PER KG",  2 "PER QUANTITY"
            int intDefaultMOPId = 0;
            int intIsVolumetricWeight = 0;


            string ConsignmentSubType = radGridConsignItems.Rows[rowIndex].Cells["ConsignmentType"].Value.ToString();
            string Description = radGridConsignItems.Rows[rowIndex].Cells["Description"].Value.ToString();
            string Qty = radGridConsignItems.Rows[rowIndex].Cells["Qty"].Value.ToString();
            string Goodsvalue = radGridConsignItems.Rows[rowIndex].Cells["Goodsvalue"].Value.ToString();
            string Weight = radGridConsignItems.Rows[rowIndex].Cells["Weight"].Value.ToString();
            string Rate = radGridConsignItems.Rows[rowIndex].Cells["Rate"].Value.ToString();
            string ConsignMentTypeID = ConsignmentSubType.Split('-')[0].Trim();

            DataTable dtConsignmentSettings = new DataView(dtConsignType, "ConsignmentSubTypeID1 = " + ConsignMentTypeID,
                                                                "", DataViewRowState.CurrentRows).ToTable();
            try
            {
                if (dtConsignmentSettings != null && dtConsignmentSettings.Rows.Count > 0)
                {
                    intIsMOPEditable = Convert.ToInt32(dtConsignmentSettings.Rows[0]["IsMOPEditable"].ToString());
                    intDefaultMOPId = Convert.ToInt32(dtConsignmentSettings.Rows[0]["DefaultMOPId"].ToString());
                    intBillingUnit = Convert.ToInt32(dtConsignmentSettings.Rows[0]["BillingUnit"].ToString());

                    intMinAllUnitChargeWeight = Convert.ToInt32(dtConsignmentSettings.Rows[0]["MinWeightAllUnits"].ToString());
                    intMinPerUnitChargeWeight = Convert.ToInt32(dtConsignmentSettings.Rows[0]["MinWeightPerUnit"].ToString());

                    intIsVolumetricWeight = Convert.ToInt32(dtConsignmentSettings.Rows[0]["IsVolumetricWeight"].ToString());
                }
            }
            catch (Exception)
            {
                intIsMOPEditable = 1;
                intDefaultMOPId = 0;
                intBillingUnit = 1;

                intMinAllUnitChargeWeight = 0;
                intMinPerUnitChargeWeight = 30;

                intIsVolumetricWeight = 0;
            }

            radGridConsignItems.Rows[rowIndex].Cells["IsVolumetricWeight"].Value = intIsVolumetricWeight;
            radGridConsignItems.Rows[rowIndex].Cells["MinWeightAllUnits"].Value = intMinAllUnitChargeWeight;
            radGridConsignItems.Rows[rowIndex].Cells["MinWeightPerUnit"].Value = intMinPerUnitChargeWeight;


            if (radGridConsignItems.Rows[rowIndex].Cells["ConsignmentTypeID"].Value != null && radGridConsignItems.Rows[rowIndex].Cells["ConsignmentTypeID"].Value != "")
            {
                if (Convert.ToInt32(radGridConsignItems.Rows[rowIndex].Cells["ConsignmentTypeID"].Value) != Convert.ToInt32(ConsignMentTypeID))
                {
                    dcmLength = 0;
                    dcmWidth = 0;
                    dcmHeight = 0;
                    dcmVolumetricWeight = 0;

                    radGridConsignItems.Rows[rowIndex].Cells["Weight"].Value = 0;
                    radGridConsignItems.Rows[rowIndex].Cells["Length"].Value = 0;
                    radGridConsignItems.Rows[rowIndex].Cells["Width"].Value = 0;
                    radGridConsignItems.Rows[rowIndex].Cells["Height"].Value = 0;
                    radGridConsignItems.Rows[rowIndex].Cells["WeightChrg"].Value = 0;
                    radGridConsignItems.Rows[0].Cells["GoodsValue"].Value = 0;
                   

                    strRate = "";

                    radGridConsignItems.Rows[rowIndex].Cells["ConsignmentTypeID"].Value = ConsignMentTypeID;
                }
            }
            else
                radGridConsignItems.Rows[rowIndex].Cells["ConsignmentTypeID"].Value = ConsignMentTypeID;

            if (intIsMOPEditable == 0)
                radGridConsignItems.Columns["MOP"].ReadOnly = true;
            else
                radGridConsignItems.Columns["MOP"].ReadOnly = false;

            if (chkIsPartySender.Checked)
            {
                int intPartyID = Convert.ToInt32(radDPartySender.SelectedValue.ToString().Split('^')[0]);

                if (dtPartyRateMaster != null && dtPartyRateMaster.Rows.Count > 0)
                {
                    DataTable dTPartyRate = new DataView(dtPartyRateMaster, "ConsignmentSubTypeID = " + ConsignMentTypeID
                                                                + " AND PartyID = " + intPartyID
                                                                + " AND FromCityID = " + intBranchCityID
                                                                + " AND ToCityID = " + Convert.ToInt32(radDDDestCity.SelectedValue),
                                                        "", DataViewRowState.CurrentRows).ToTable();

                    if (dTPartyRate != null && dTPartyRate.Rows.Count > 0 && Convert.ToInt32(dTPartyRate.Rows[0]["BillingUnit"].ToString()) > 0)
                    {
                        intBillingUnit = Convert.ToInt32(dTPartyRate.Rows[0]["BillingUnit"].ToString());
                    }
                }
            }

            if (intBillingUnit == 2)  // Per Quantity
            {
                /***** Pradeep:2018-02-26: Requirement from STA to make Weight(Act) and Weight(Chg) editable in unit type of consigment *********/
                //if (intCompanyID == 406)
                //    radGridConsignItems.Columns["GoodsValue"].ReadOnly = true; // 4

                //radGridConsignItems.Columns["Weight"].ReadOnly = true; //5
                //radGridConsignItems.Columns["WeightChrg"].ReadOnly = true; //6
                radGridConsignItems.Rows[rowIndex].Cells["WeightVolume"].ReadOnly = true;

                //radGridConsignItems.Rows[rowIndex].Cells["Weight"].Value = "0";
                //radGridConsignItems.Rows[rowIndex].Cells["WeightChrg"].Value = "0";
                radGridConsignItems.Rows[rowIndex].Cells["WeightVolume"].Value = "0";
            }
            else  // Per KG
            {
                if (intCompanyID == 406)
                    radGridConsignItems.Columns["GoodsValue"].ReadOnly = false;
                radGridConsignItems.Columns["Weight"].ReadOnly = false;
                radGridConsignItems.Columns["WeightChrg"].ReadOnly = false;

                if (intIsVolumetricWeight == 1)
                {
                    radGridConsignItems.Columns["Weight"].ReadOnly = true;
                    radGridConsignItems.Rows[rowIndex].Cells["WeightVolume"].ReadOnly = false;
                }
                else
                {
                    radGridConsignItems.Columns["Weight"].ReadOnly = false;
                    radGridConsignItems.Rows[rowIndex].Cells["WeightVolume"].ReadOnly = true;
                }

                if (celIndex == 0)
                    radGridConsignItems.Rows[rowIndex].Cells["MOP"].Value = "0"; //1
            }



            if (celIndex == 0)  // Consignment Type
            {
                radGridConsignItems.Rows[rowIndex].Cells["MOP"].Value = intDefaultMOPId.ToString();

                if (radGridConsignItems.Columns["MOP"].IsVisible && !radGridConsignItems.Columns["MOP"].ReadOnly)
                    radGridConsignItems.Rows[rowIndex].Cells["MOP"].BeginEdit();
                else
                    radGridConsignItems.Rows[rowIndex].Cells["Description"].BeginEdit();

                // Assigne Party Master Rate
                if (blnIsSTACargoCompany)
                {
                    if (chkIsPartySender.Checked)
                    {
                        int intPartyID = Convert.ToInt32(radDPartySender.SelectedValue.ToString().Split('^')[0]);

                        if (dtPartyRateMaster != null && dtPartyRateMaster.Rows.Count > 0)
                        {
                            DataTable dTPartyRate = new DataView(dtPartyRateMaster, "ConsignmentSubTypeID = " + ConsignMentTypeID
                                                                        + " AND PartyID = " + intPartyID
                                                                        + " AND FromCityID = " + intBranchCityID
                                                                        + " AND ToCityID = " + Convert.ToInt32(radDDDestCity.SelectedValue),
                                                                "", DataViewRowState.CurrentRows).ToTable();

                            if (dTPartyRate != null && dTPartyRate.Rows.Count > 0)
                            {
                                strRate = dTPartyRate.Rows[0]["Rate"].ToString();
                                strPartyRate = strRate;
                            }
                        }


                        if (strRate == "" && dtPartyRateMaster != null && dtPartyRateMaster.Rows.Count > 0)
                        {
                            DataTable dTPartyRate = new DataView(dtPartyRateMaster, "ConsignmentSubTypeID = " + ConsignMentTypeID
                                                                        + " AND PartyID = " + intPartyID
                                                                        + " AND FromCityID = 0"
                                                                        + " AND ToCityID = 0",
                                                                "", DataViewRowState.CurrentRows).ToTable();

                            if (dTPartyRate != null && dTPartyRate.Rows.Count > 0)
                            {
                                strRate = dTPartyRate.Rows[0]["Rate"].ToString();
                                strPartyRate = strRate;
                            }
                        }
                    }

                }

                if (strRate == "" && dtRateMaster != null && dtRateMaster.Rows.Count > 0)
                {
                    DataTable dTRate = new DataView(dtRateMaster, "ConsignmentSubTypeID = " + ConsignMentTypeID
                                                                + " AND FromCityID = " + intBranchCityID
                                                                + " AND ToCityID = " + Convert.ToInt32(radDDDestCity.SelectedValue),
                                                        "", DataViewRowState.CurrentRows).ToTable();

                    if (dTRate != null && dTRate.Rows.Count > 0)
                    {
                        strRate = dTRate.Rows[0]["Rate"].ToString();
                        strPartyRate = strRate;
                    }
                }

                if (strRate == "")
                {
                    strRate = ConsignmentSubType.Split('-')[1];
                }

                if (is_direct_freight_company)
                    radGridConsignItems.Rows[rowIndex].Cells["Rate"].Value = "0";
                else
                    radGridConsignItems.Rows[rowIndex].Cells["Rate"].Value = strRate;

                radGridConsignItems.Rows[rowIndex].Cells["Freight"].Value = "0";
            }
            else if (celIndex == 1) // MOP
            {
                if (radGridConsignItems.Columns["Description"].IsVisible)
                    radGridConsignItems.Rows[rowIndex].Cells["Description"].BeginEdit();
                else
                    radGridConsignItems.Rows[rowIndex].Cells["Qty"].BeginEdit();

            }
            else if (celIndex == 2) // Desc
            {
                if (radGridConsignItems.Columns["Qty"].IsVisible)
                    radGridConsignItems.Rows[rowIndex].Cells["Qty"].BeginEdit();
                else
                    radGridConsignItems.Rows[rowIndex].Cells["GoodsValue"].BeginEdit();
            }
            else if (celIndex == 3) // Unit
            {
                if (radGridConsignItems.Columns["GoodsValue"].IsVisible && !radGridConsignItems.Columns["GoodsValue"].ReadOnly)
                    radGridConsignItems.Rows[rowIndex].Cells["GoodsValue"].BeginEdit();
                else if (radGridConsignItems.Columns["Weight"].IsVisible && !radGridConsignItems.Columns["Weight"].ReadOnly)
                    radGridConsignItems.Rows[rowIndex].Cells["Weight"].BeginEdit();
                else
                    radGridConsignItems.Rows[rowIndex].Cells["Rate"].BeginEdit();

                if (blnIsSTACargoCompany)
                    SetDocumentCharge(Convert.ToInt32(radDPayType.SelectedValue));

                if (blnIsSTACargoCompany && Convert.ToInt32(radDDBookingBranch.SelectedValue) == 24172 && txtCartageDel.Visible)
                {
                    txtCartageDel.Text = Convert.ToString(Convert.ToInt32(e.Row.Cells["Qty"].Value) * 100);
                }
            }
            else if (celIndex == 4) // Goods Value
            {
                if (radGridConsignItems.Columns["Weight"].IsVisible && !radGridConsignItems.Columns["Weight"].ReadOnly)
                    radGridConsignItems.Rows[rowIndex].Cells["Weight"].BeginEdit();
                else if (radGridConsignItems.Columns["WeightVolume"].IsVisible && intIsVolumetricWeight == 1) //!radGridConsignItems.Columns["WeightVolume"].ReadOnly)
                    radGridConsignItems.Rows[rowIndex].Cells["WeightVolume"].BeginEdit();

                else if (radGridConsignItems.Columns["WeightChrg"].IsVisible && !radGridConsignItems.Columns["WeightChrg"].ReadOnly)
                    radGridConsignItems.Rows[rowIndex].Cells["WeightChrg"].BeginEdit();
                else
                    radGridConsignItems.Rows[rowIndex].Cells["Rate"].BeginEdit();

                CalcInsurance();
            }
            else if (celIndex == 5)  // Actual Weight
            {
                if (blnIsSTACargoCompany)
                {
                    radGridConsignItems.Columns["WeightChrg"].ReadOnly = true;  //6

                    /****** Pradeep:2018-05-09:  Actual weight can not be zero as requested by Bhagirath (STA) on 8th May 2018*************/
                    if (!(radGridConsignItems.Columns["Weight"].ReadOnly) && Convert.ToDecimal(radGridConsignItems.Rows[rowIndex].Cells["Weight"].Value) <= 0 && intBillingUnit == 1)
                    {
                        MessageBox.Show("Actual weight can not be zero. Please enter weight again.");
                        radGridConsignItems.Rows[rowIndex].Cells["Weight"].BeginEdit();
                        return;
                    } 

                    if (radGridConsignItems.Rows[rowIndex].Cells["Weight"].Value.ToString() == "")
                        radGridConsignItems.Rows[rowIndex].Cells["Weight"].Value = 0;

                    int intQty = Convert.ToInt32(e.Row.Cells["Qty"].Value.ToString());

                    int intActWeight = Convert.ToInt32(radGridConsignItems.Rows[rowIndex].Cells["Weight"].Value);

                    int intCalChrgWeight = 0;

                    if (intMinPerUnitChargeWeight > 0)
                        intCalChrgWeight = intQty * intMinPerUnitChargeWeight;
                    else
                        intCalChrgWeight = intMinAllUnitChargeWeight;

                    //if (intBillingUnit == 2)
                    //    intCalChrgWeight = intActWeight;
                    
                    if (intCalChrgWeight < intActWeight)
                    {
                        radGridConsignItems.Rows[rowIndex].Cells["WeightChrg"].Value = intActWeight;
                    }
                    else
                    {
                        radGridConsignItems.Rows[rowIndex].Cells["WeightChrg"].Value = intCalChrgWeight;
                    }
                }

                if (radGridConsignItems.Columns["WeightChrg"].IsVisible && !radGridConsignItems.Columns["WeightChrg"].ReadOnly)
                    radGridConsignItems.Rows[rowIndex].Cells["WeightChrg"].BeginEdit();
                else
                    radGridConsignItems.Rows[rowIndex].Cells["Rate"].BeginEdit();
            }
            else if (celIndex == 7 && AllowRateChange) // Charge Weight
            {
                if (is_direct_freight_company)
                    radGridConsignItems.Rows[rowIndex].Cells["Freight"].BeginEdit();
                else
                    radGridConsignItems.Rows[rowIndex].Cells["Rate"].BeginEdit();
            }

            else if (celIndex == 8 || celIndex == 9)  // Rate || Fright     // else if ((!AllowRateChange && celIndex == 6) || celIndex == 7 || celIndex == 8)
            {
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
                        double dblPartyRate = Convert.ToDouble((strPartyRate == "" ? "0" : strPartyRate));

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
                                    else
                                    {
                                        if (dblPartyRate != 0 && dblPartyRate != dblNewFare && radDPayType.SelectedItem.Value.ToString() == "5")
                                        {
                                            DialogResult dr1 = MessageBox.Show("Rate entered for Party Booking does not match the assigned rate. Are you sure you want to continue?", "Mismatch in Party Rate", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                            if (dr1 == DialogResult.No)
                                            {
                                                radGridConsignItems.Rows[rowIndex].Cells["Rate"].BeginEdit();
                                                return;
                                            }
                                        }
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
                                    else
                                    {
                                        if (dblPartyRate != 0 && dblPartyRate != dblNewFare && radDPayType.SelectedItem.Value.ToString() == "5")
                                        {
                                            DialogResult dr1 = MessageBox.Show("Rate entered for Party Booking does not match the assigned rate. Are you sure you want to continue?", "Mismatch in Party Rate", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                            if (dr1 == DialogResult.No)
                                            {
                                                radGridConsignItems.Rows[rowIndex].Cells["Rate"].BeginEdit();
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (dblPartyRate != 0 && dblPartyRate != dblNewFare && radDPayType.SelectedItem.Value.ToString() == "5")
                                {
                                    DialogResult dr1 = MessageBox.Show("Rate entered for Party Booking does not match the assigned rate. Are you sure you want to continue?", "Mismatch in Party Rate", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                    if (dr1 == DialogResult.No)
                                    {
                                        radGridConsignItems.Rows[rowIndex].Cells["Rate"].BeginEdit();
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    double dblNewFare = Convert.ToDouble(e.Row.Cells["Rate"].Value.ToString());
                    double dblOldFare = Convert.ToDouble((strRate == "" ? "0" : strRate));
                    double dblPartyRate = Convert.ToDouble((strPartyRate == "" ? "0" : strPartyRate));

                    if (dblPartyRate != 0 && dblPartyRate != dblNewFare && radDPayType.SelectedItem.Value.ToString() == "5")
                    {
                        DialogResult dr1 = MessageBox.Show("Rate entered for Party Booking does not match the assigned rate. Are you sure you want to continue?", "Mismatch in Party Rate", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (dr1 == DialogResult.No)
                        {
                            radGridConsignItems.Rows[rowIndex].Cells["Rate"].BeginEdit();
                            return;
                        }
                    }
                }

                string qty = e.Row.Cells["Qty"].Value.ToString();
                string weightchrg = e.Row.Cells["WeightChrg"].Value.ToString();
                string rate = e.Row.Cells["Rate"].Value.ToString();
                string consignmentype = e.Row.Cells["ConsignmentType"].Value.ToString().Split('-')[0].ToString();


                string Freight = "";

                if (intBillingUnit == 2)  // Per Quantity
                {
                    Freight = (Convert.ToInt32(qty) * Convert.ToDouble(rate)).ToString();
                }
                else  // Per KG
                    Freight = (Convert.ToDouble(weightchrg) * Convert.ToDouble(rate)).ToString();

                double dblFreight = Math.Round(Convert.ToDouble(Freight));

                Freight = Convert.ToString(dblFreight);

                if (celIndex == 8)
                    radGridConsignItems.Rows[rowIndex].Cells["Freight"].Value = Freight;

                DataView dv = dtGriSourse.DefaultView;

                txtFreightChg.Text = dtGriSourse.Compute("SUM(Freight)", dv.RowFilter).ToString();

                if (is_direct_freight_company)
                {
                    if ((!AllowRateChange && celIndex == 5) || celIndex == 7)
                    {
                        radGridConsignItems.Rows[rowIndex].Cells["Freight"].BeginEdit();
                        return;
                    }
                }

                if (celIndex == 8)
                    radGridConsignItems.Rows[rowIndex].Cells["Freight"].BeginEdit();

                if (celIndex == 9)
                {
                    qty = e.Row.Cells["Qty"].Value.ToString();
                    weightchrg = e.Row.Cells["WeightChrg"].Value.ToString();
                    Freight = e.Row.Cells["Freight"].Value.ToString();

                    if (intBillingUnit == 2)  // Per Quantity
                    {
                        if (qty == "0" || qty == "")
                        {
                            MessageBox.Show("Please Enter Unit.");
                            radGridConsignItems.Rows[rowIndex].Cells["Qty"].BeginEdit();
                            return;
                        }
                        else
                        {
                            try
                            {
                                double rt = (Convert.ToDouble(Freight) / Convert.ToInt32(qty));
                                if (!double.IsNaN(rt) && !double.IsInfinity(rt))
                                    rate = rt.ToString();
                                else
                                    rate = "0";
                            }
                            catch (Exception ex)
                            {
                                rate = "0";
                            }
                        }
                    }
                    else // Per KG
                    {
                        try
                        {
                            double rt = (Convert.ToDouble(Freight) / Convert.ToDouble(weightchrg));
                            if (!double.IsNaN(rt) && !double.IsInfinity(rt))
                                rate = rt.ToString();
                            else
                            {
                                rate = "0";
                            }
                        }
                        catch (Exception ex)
                        {
                            rate = "0";
                        }
                    }

                    radGridConsignItems.Rows[rowIndex].Cells["Rate"].Value = rate;

                    if (Convert.ToDouble(rate) <= 0 && radDPayType.SelectedValue.ToString() != "3" && !is_direct_freight_company)
                    {
                        MessageBox.Show("Please Enter Rate.");
                        radGridConsignItems.Rows[0].Cells["Rate"].BeginEdit();
                        goto SkipProc;
                    }

                    if (intBillingUnit == 1 && (Convert.ToDecimal(radGridConsignItems.Rows[rowIndex].Cells["WeightChrg"].Value) < Convert.ToDecimal(radGridConsignItems.Rows[rowIndex].Cells["Weight"].Value)))
                    {
                        MessageBox.Show("Actual weight can not be greater than Charged weight. Please enter actual weight again!");
                        radGridConsignItems.Rows[rowIndex].Cells["Weight"].BeginEdit();
                        goto SkipProc;
                    }

                    DialogResult dr;

                    if ((blnIsSTACargoCompany || is_direct_freight_company) && intCompanyID != 1)
                        dr = DialogResult.No;
                    else
                        dr = MessageBox.Show("Do you want to add another consignment?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                    if (dr == DialogResult.Yes)
                    {
                        AddNewRow("0", "0", "", "0", "0", "0", "0", 0, 0, 0, 0, 0, 0);
                        fillGrid();

                        radGridConsignItems.Rows[rowIndex].IsSelected = false;
                        radGridConsignItems.Rows[rowIndex + 1].IsSelected = true;

                        radGridConsignItems.Rows[rowIndex].Cells[celIndex].IsSelected = false;
                        radGridConsignItems.Rows[rowIndex + 1].Cells["ConsignmentType"].IsSelected = true;
                        radGridConsignItems.Rows[rowIndex + 1].Cells["ConsignmentType"].BeginEdit();
                    }
                    else
                    {
                        if (txtDeliveryChg.Enabled && txtDeliveryChg.Visible)
                            txtDeliveryChg.Focus();

                        else if (txtCartage.Enabled && txtCartage.Visible)
                            txtCartage.Focus();
                        else if (txtDocumentChg.Enabled && txtDocumentChg.Visible)
                            txtDocumentChg.Focus();
                        else if (txtInsurance.Enabled && txtInsurance.Visible)
                            txtInsurance.Focus();

                        else if (txtServiceTax.Enabled && txtServiceTax.Visible)
                            txtServiceTax.Focus();
                        else if (radDVehicleNos.Visible)
                            radDVehicleNos.Focus();
                        else if (txtHamaliChg.Visible)
                            txtHamaliChg.Focus();
                        else
                            txtComment.Focus();

                        setAdditionalCharges();
                        TotalAmount();
                    }
                }
            }
        SkipProc:
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
            if (!blnIsOfflineMode && chkInsurance.Visible && chkInsurance.Checked)
            {
                decimal total_goods_value = 0;
                for (int i = 0; i < radGridConsignItems.Rows.Count; i++)
                {
                    total_goods_value += Convert.ToDecimal(radGridConsignItems.Rows[i].Cells["Qty"].Value.ToString());
                }
                txtInsurance.Text = ((total_goods_value / 100) * (decimal)0.5).ToString();
            }
        }

        public void setAdditionalCharges()
        {
            try
            {
                if (!blnIsOfflineMode && txtInsurance.Visible)
                {
                    //radGridConsignItems.Rows[rowIndex].Cells
                    string CSTID = "";
                    string MGV = "";
                    int TotalQty = 0;
                    for (int i = 0; i < radGridConsignItems.Rows.Count; i++)
                    {
                        string ConsignmentSubType = radGridConsignItems.Rows[i].Cells["ConsignmentType"].Value.ToString();
                        string ConsignMentTypeID = ConsignmentSubType.Split('-')[0].Trim();
                        string Goodsvalue = radGridConsignItems.Rows[i].Cells["Goodsvalue"].Value.ToString();
                        int Qty = Convert.ToInt32(radGridConsignItems.Rows[i].Cells["Qty"].Value.ToString());

                        CSTID += ConsignMentTypeID + ",";
                        MGV += Goodsvalue + ",";
                        TotalQty += Qty;
                    }
                    CSTID = CSTID.Trim(new char[] { ',' });
                    MGV = MGV.Trim(new char[] { ',' });

                    DataSet ds = App_Code.Cargo.GetAdditionalCharges(intCompanyID, intBranchID, CSTID, "B", Convert.ToInt16(radDPayType.SelectedValue.ToString()), MGV);
                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        int InsuranceCharges = Convert.ToInt16(ds.Tables[0].Rows[0]["InsuranceCharges"]);
                        int AdditionalUnitInsuranceCharges = Convert.ToInt16(ds.Tables[0].Rows[0]["InsuranceAdditionalUnitAmt"]);

                        if (txtInsurance.Visible)
                        {
                            if (InsuranceCharges > 0)
                            {
                                //if (intCompanyID != 2671) // Shree Shyam Travels And Cargo (ABD)
                                if (AdditionalUnitInsuranceCharges > 0)
                                {
                                    TotalQty = TotalQty - 1;
                                    InsuranceCharges = InsuranceCharges + (TotalQty * AdditionalUnitInsuranceCharges);
                                    //InsuranceCharges = InsuranceCharges + (TotalQty * 10);
                                }
                                txtInsurance.Text = InsuranceCharges.ToString();
                            }
                            else
                                txtInsurance.Text = "0";
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        private void radGridConsignItems_Enter(object sender, EventArgs e)
        {
            //radGridConsignItems.Rows[0].Cells[0].BeginEdit();
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
                    txtSearchPartySender.Visible = true;
            
                    if (intCompanyID == 406)
                        radDModeofPayment.Enabled = false;


                    if (IsPartyForSenderReceiver)
                    {
                        chkIsPartySender.Enabled = true;
                        chkIsPartyReceiver.Enabled = true;

                        //chkIsPartyReceiver.Checked = false;
                        //chkIsPartyReceiver.Enabled = true;
                        //chkIsPartySender.Enabled = false;
                        //radDModeofPayment.Enabled = false;
                    }
                    else
                    {
                        chkIsPartySender.Enabled = false;
                        chkIsPartyReceiver.Enabled = true;
                        chkIsPartyReceiver.Checked = false;

                        if (radDPayType.Items.Count > 0 && radDPayType.SelectedItem.Text.ToUpper() == "PAID")
                            radDModeofPayment.Enabled = true;
                        else
                            radDModeofPayment.Enabled = false;
                    }

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
                    txtSearchPartySender.Visible = false;
                    radDModeofPayment.Enabled = false;
                    radDPartySender.Visible = false;
                    chkSenderMobileGetData.Enabled = true;
                    txtNameSender.Text = "";
                    txtMobileNo.Text = "";
                    txtNameSender.Enabled = true;
                    txtMobileNo.Enabled = true;
                    //txtSenderGSTN.Text = "";
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
                    txtSearchPartyReceiver.Visible = true;

                    if (IsPartyForSenderReceiver)
                    {
                        if (intCompanyID != 406)
                            chkIsPartySender.Enabled = true;
                        chkIsPartyReceiver.Enabled = true;

                        //chkIsPartySender.Checked = false;
                        //chkIsPartySender.Enabled = true;
                        //chkIsPartyReceiver.Enabled = false;
                    }
                    else
                    {
                        chkIsPartySender.Enabled = true;
                        chkIsPartyReceiver.Enabled = false;
                        chkIsPartySender.Checked = false;
                    }

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
                    txtSearchPartyReceiver.Visible = false;
                    radDPartyReceiver.Visible = false;
                    txtNameReceiver.Text = "";
                    txtMobileNoReceiver.Text = "";
                    txtAddressReceiver.Text = radDDDestCity.Text;
                    txtNameReceiver.Enabled = true;
                    chkRecMobileGetData.Enabled = true;
                    txtMobileNoReceiver.Enabled = true;
                    txtReceiverGSTN.Text = "";
                    //txtAddressReceiver.Enabled = true;
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
                int intSelPartyID = 0;
                if (chkIsPartySender.Checked)
                {
                    string[] str = radDPartySender.SelectedValue.ToString().Split('^');

                    txtNameSender.Text = radDPartySender.SelectedItem.Text;
                    txtMobileNo.Text = str[1];
                    intSelPartyID = Convert.ToInt32(str[0]);

                    try
                    {
                        txtSenderGSTN.Text = str[5];
                    }
                    catch (Exception)
                    {
                        txtSenderGSTN.Text = "";
                    }
                    //radDDCStaxPaidBy.SelectedValue = str[4];

                    if (intCompanyID == 168)//royal travels nagpur
                    {
                        int intServiceTaxPayer = Convert.ToInt32(str[3]);

                        if (intServiceTaxPayer == 2 || intServiceTaxPayer == 3)
                            ((GridViewDecimalColumn)radGridConsignItems.Columns["Freight"]).Maximum = 9999999;
                        else
                            ((GridViewDecimalColumn)radGridConsignItems.Columns["Freight"]).Maximum = dcmMinTaxAmount; // 750;
                    }


                    DataTable dtPartyFrom = new DataTable();

                    DataTable dtParty = Common.GetLuggageParties("B");

                    if (dtParty != null && dtParty.Rows.Count > 0)
                    {
                        dtPartyFrom = new DataView(dtParty, "PartyID=" + intSelPartyID, "", DataViewRowState.CurrentRows).ToTable();

                        if (dtPartyFrom != null && dtPartyFrom.Rows.Count > 0)
                        {
                            intisBkgCreditLimit = Convert.ToInt32(dtPartyFrom.Rows[0]["IsBookingCreditLimit"].ToString());
                        }
                    }
                    else
                        intisBkgCreditLimit = 0;

                    if (intCompanyID == 1 || intCompanyID == 406)
                    {
                        if (intisBkgCreditLimit == 0 && radDPayType.SelectedValue.ToString() == "5")
                            MessageBox.Show("You cannot do On-Account booking with selected party, Please enable Booking Credit Limit for party.", "Ooops..", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
                else
                {
                    radDPartySender.Visible = false;
                    txtNameSender.Text = "";
                    txtMobileNo.Text = "";
                    txtNameSender.Enabled = true;
                    txtMobileNo.Enabled = true;
                    //radDDCStaxPaidBy.SelectedValue = "";
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
                    //txtAddressReceiver.Enabled = true;
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
                SetDocumentCharge(Convert.ToInt32(radDPayType.SelectedValue));

                txtDeliveryChg.Enabled = true;
                txtCartage.Enabled = true;

                txtDocumentChg.Enabled = true;
                txtInsurance.Enabled = true;

                txtServiceTax.Enabled = false;

                ResetFTLChanges();

                SetPartyControls();

                if (intCompanyID == 168)//royal travels nagpur
                    ((GridViewDecimalColumn)radGridConsignItems.Columns["Freight"]).Maximum = dcmMinTaxAmount; //750;
                else
                    ((GridViewDecimalColumn)radGridConsignItems.Columns["Freight"]).Maximum = 9999999;

                if (!blnIsOfflineMode)
                {
                    ShowHideManualLRBox(false);

                    ShowHideOfflineBookingUser(false);
                }
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

                        ShowHideOfflineBookingUser(true);
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

                        ShowHideOfflineBookingUser(true);
                    }
                }
                else if (radDPayType.SelectedValue.ToString() == "3")
                {
                    grpBxSenderInfo.BackColor = objColorFOC;
                    grpBxReceiverInfo.BackColor = objColorFOC;
                    grpBxConsignmentItems.BackColor = objColorFOC;
                    grpBXPayment.BackColor = objColorFOC;
                    grpCharges.BackColor = objColorFOC;


                    txtDeliveryChg.Text = "0";
                    txtCartage.Text = "0";
                    //  txtDocumentChg.Text = "0";
                    txtInsurance.Text = "0";

                    txtServiceTax.Text = "0";


                    txtDeliveryChg.Enabled = false;
                    txtCartage.Enabled = false;
                    txtDocumentChg.Enabled = false;
                    txtInsurance.Enabled = false;


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

                if (intCompanyID == 406)
                {
                    txtInsurance.Enabled = false;
                    txtDocumentChg.Enabled = false;
                }

                txtServiceTax.Text = "0";

                if (lblStaxPaidBy.Visible && radDDCStaxPaidBy.Visible && is_stax_company && is_GSTN_branch == 1)
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

                //tblLPManualLR.ColumnStyles[0].Width = 300;
                //tblLPManualLR.ColumnStyles[1].Width = 0;

            }
            else
            {
                lblManualLR.Visible = ToShow;
                txtManualLR.Visible = ToShow;

                //if (ToShow)
                //{
                //    tblLPManualLR.ColumnStyles[0].Width = 122;
                //    tblLPManualLR.ColumnStyles[1].Width = 45;
                //}
                //else
                //{
                //    tblLPManualLR.ColumnStyles[0].Width = 300;
                //    tblLPManualLR.ColumnStyles[1].Width = 0;
                //}
            }
        }

        public void ShowHideOfflineBookingUser(Boolean ToShow)
        {
            if (intCompanyID != 406)
            {
                if (radBSave.Text == "Update")
                {
                    lblofflineBookingUser.Visible = false;
                    radDDOfflineBookingUser.Visible = false;

                    //tlpOfflineBookingUser.ColumnStyles[0].Width = 300;
                    //tlpOfflineBookingUser.ColumnStyles[1].Width = 0;

                }
                else
                {
                    lblofflineBookingUser.Visible = ToShow;
                    radDDOfflineBookingUser.Visible = ToShow;

                    if (ToShow)
                    {
                        //tlpOfflineBookingUser.ColumnStyles[0].Width = 200;
                        //tlpOfflineBookingUser.ColumnStyles[1].Width = 45;
                    }
                    else
                    {
                        //tlpOfflineBookingUser.ColumnStyles[0].Width = 300;
                        //tlpOfflineBookingUser.ColumnStyles[1].Width = 0;
                    }
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

                    if (!blnIsOfflineMode && (intCompanyID == 66 || intCompanyID == 184 || intCompanyID == 805 || intCompanyID == 2649 || intCompanyID == 2650 || intCompanyID == 322))  // Jain Travels related Changes
                    {
                        if (txtAmount.Text.ToString().Trim() != "" && Convert.ToDouble(txtAmount.Text) >= Convert.ToDouble(dcmMinTaxAmount)) //750)
                        {
                            MessageBox.Show("Booking cannot be done for Gross Amount greater than " + dcmMinTaxAmount + "", "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    if (!ValidateValue())
                        return;

                    DialogResult dr = MessageBox.Show("Are you sure you want to Update this luggage?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    double dblFreightChg = Convert.ToDouble((txtFreightChg.Text != "" ? txtFreightChg.Text : "0"));
                    double dblDocumentChg = Convert.ToDouble((txtDocumentChg.Text != "" ? txtDocumentChg.Text : "0"));
                    double dblServiceTax = Convert.ToDouble((txtServiceTax.Text != "" ? txtServiceTax.Text : "0"));
                    double dblInsurance = Convert.ToDouble((txtInsurance.Text != "" ? txtInsurance.Text : "0"));
                    double dblCollectionChg = 0;// Convert.ToDouble((txtCollectionChg.Text != "" ? txtCollectionChg.Text : "0"));
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

                    if (!blnIsOfflineMode && (intCompanyID == 66 || intCompanyID == 184 || intCompanyID == 805 || intCompanyID == 2649 || intCompanyID == 2650 || intCompanyID == 322))  // Jain Travels related Changes
                    {
                        if (txtAmount.Text.ToString().Trim() != "" && Convert.ToDouble(txtAmount.Text) >= Convert.ToDouble(dcmMinTaxAmount)) // 750)
                        {
                            MessageBox.Show("Booking cannot be done for Gross Amount greater than " + dcmMinTaxAmount + "", "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    if (!ValidateValue())
                        return;

                    //if (chkCrossing.Checked && Convert.ToDecimal("0" + txtCartage.Text) < 1)
                    //{
                    //    MessageBox.Show("CARTAGE required for CROSSING", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //    return;
                    //}

                    if (txtCollCartageRemark.Visible)
                    {
                        if (Convert.ToInt64(txtCartage.Text) > 0 && txtCollCartageRemark.Text == "Cartage - Mobile and Comment")
                        {
                            MessageBox.Show("Please enter Collection Cartage Remarks");
                            txtCollCartageRemark.Focus();
                            return;
                        }
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
                        double dblCollectionChg = 0;// Convert.ToDouble((txtCollectionChg.Text != "" ? txtCollectionChg.Text : "0"));
                        double dblDeliveryChg = Convert.ToDouble((txtDeliveryChg.Text != "" ? txtDeliveryChg.Text : "0"));
                        double dblCartage = Convert.ToDouble((txtCartage.Text != "" ? txtCartage.Text : "0"));
                        double dblCartageDel = Convert.ToDouble((txtCartageDel.Text != "" ? txtCartageDel.Text : "0"));

                        double dblHamaliChg = Convert.ToDouble((txtHamaliChg.Text != "" ? txtHamaliChg.Text : "0"));

                        if (!txtDeliveryChg.Enabled && !txtDeliveryChg.Visible)
                        {
                            dblDeliveryChg = 0;
                        }

                        double dblNetAmount = dblFreightChg + dblDeliveryChg + dblDocumentChg + dblServiceTax + dblInsurance + dblCollectionChg + dblCartage + dblCartageDel + dblHamaliChg;

                        int intModeOfPayment = Convert.ToInt32(radDModeofPayment.SelectedValue);

                        int intDeliveryType = 0;
                        int intCollectionType = 0;

                        if (raddDeliveryType.Visible && lblDeliveryType.Visible)
                        {
                            intDeliveryType = Convert.ToInt32(raddDeliveryType.SelectedValue);
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

                        double dblDoorDelCharges = 0;
                        if (IsOrangeTypeDisplay)
                        {
                            dblNetAmount = dblNetAmount - dblHamaliChg;

                            dblDoorDelCharges = dblInsurance;
                            dblInsurance = 0;
                        }

                        string strManualLR = "";

                        if (txtManualLR.Visible)
                            strManualLR = txtManualLR.Text;

                        int errDtCnt = 0;

                        string errMsgs = "";

                        string strBookingDetailsData = "";
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
                        string strAllMOPID = "";

                        foreach (DataRow drw in dtGriSourse.Rows)
                        {
                            intBookingID = 0;
                            int ConsignmentSubTypeID = Convert.ToInt32(drw["ConsignmentTypeID"].ToString().Split('-')[0]);
                            int MOPID = Convert.ToInt32(drw["MOPID"].ToString());
                            string strDescription = drw["Description"].ToString();
                            int intQuantity = Convert.ToInt32(drw["Qty"]);
                            int intIsVolumetricWeight = 0;
                            int intLength = 0;
                            int intWidth = 0;
                            int intHeight = 0;

                            if (drw["IsVolumetricWeight"] != null)
                            {
                                intIsVolumetricWeight = Convert.ToInt32(drw["IsVolumetricWeight"]);
                            }

                            if (drw["Length"] != null)
                            {
                                intLength = Convert.ToInt32(drw["Length"]);
                            }

                            if (drw["Width"] != null)
                            {
                                intWidth = Convert.ToInt32(drw["Width"]);
                            }

                            if (drw["Height"] != null)
                            {
                                intHeight = Convert.ToInt32(drw["Height"]);
                            }

                            double dblGoodsValue;
                            double dblActualWeight = 0;
                            double dblChargedWeight = 0;
                            try
                            {
                                dblGoodsValue = Convert.ToDouble((drw["Goodsvalue"].ToString() == "" ? 0 : drw["Goodsvalue"]));
                                dblActualWeight = Convert.ToDouble((drw["Weight"].ToString() == "" ? 0 : drw["Weight"])); //Convert.ToDouble(drw["Weight"]);
                                dblChargedWeight = Convert.ToDouble((drw["WeightChrg"].ToString() == "" ? 0 : drw["WeightChrg"]));
                            }
                            catch (Exception ex)
                            {
                                dblGoodsValue = 0;
                            }
                            string strVolume = "0";

                            if (intIsVolumetricWeight == 1)
                                strVolume = "l=" + intLength + ",b=" + intWidth + ",h=" + intHeight;  // Warning!! Don't change this format
                            else
                                strVolume = "0";

                            double dblActualRate = Convert.ToDouble(drw["Rate"]);
                            double dblRate = Convert.ToDouble(drw["Rate"]);
                            double dblFreight = Convert.ToDouble(drw["Freight"]);
                            double dblAmount = Convert.ToDouble(drw["Freight"]);
                            int intCurrentCity = Common.GetBranchCityID(); //Convert.ToInt32(radDDBookingCity.SelectedValue);
                            int intCurrentBranch = Common.GetBranchID(); //Convert.ToInt32(radDDBookingBranch.SelectedValue);

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
                                strAllMOPID += "||" + MOPID;
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
                            strAllMOPID = strAllMOPID.Substring(2);
                        }

                        int intCrossingCity, intCrossingBranch;
                        intCrossingBranch = intCrossingCity = 0;
                        //if (chkCrossing.Checked)
                        //{
                        //    intCrossingCity = Convert.ToInt32(radDDCrossingCity.SelectedValue);
                        //    intCrossingBranch = Convert.ToInt32(radDDCrossingBranch.SelectedValue);
                        //}

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

                        int intBookCityID = 0;
                        int intBookBranchID = 0;
                        if (blnIsOfflineMode)
                        {
                            intBookCityID = Convert.ToInt32(radDDBookingCity.SelectedValue);
                            intBookBranchID = Convert.ToInt32(radDDBookingBranch.SelectedValue);
                        }
                        else
                        {
                            intBookCityID = Common.GetBranchCityID();
                            intBookBranchID = Common.GetBranchID();
                        }

                        string strOfflineVehicle = "";
                        int intOfflineVehicleID = 0;

                        if (radDVehicleNos.Items.Count > 0)
                        {
                            //if (radDVehicleNos.SelectedValue != null && radDVehicleNos.SelectedValue.ToString() != "--Select--")
                            if (radDVehicleNos.SelectedItem.Text.ToString() != null && radDVehicleNos.SelectedItem.Text.ToString() != "--Select--")
                            {
                                strOfflineVehicle = radDVehicleNos.SelectedItem.Text.ToString(); // radDVehicleNos.SelectedValue.ToString();
                                intOfflineVehicleID = Convert.ToInt32(radDVehicleNos.SelectedValue.ToString());
                            }
                            else
                            {
                                strOfflineVehicle = "";
                                intOfflineVehicleID = 0;
                            }
                        }

                        int intBookUserID = intUserID;
                        if (blnIsOfflineMode && intCompanyID != 406) // Ktn - For STA take Login User as Offline Booking User as per Afzal
                        {
                            intBookUserID = Convert.ToInt32(radDDOfflineBookingUser.SelectedValue);
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

                        DataTable dt = App_Code.Cargo.CargoBookingsInsertUpdateBulkCrossingV3(
                                        intBookingID, intCompanyID, intBookCityID,
                                        intBookBranchID,
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
                                        txtComment.Text, intBookUserID,
                                        intModeOfPayment, Common.GetCacheGUID(),
                                        dblDeliveryChg, intDeliveryType, intCollectionType,
                                        dblHamaliChg, dblDoorDelCharges,
                                        strManualLR, 0, 0, Common.GetLogID(), staxPaidBy,
                                        txtLRNo.Text, dblCartageDel,
                                        (txtCollCartageRemark.Text == "Cartage - Mobile and Comment" ? "" : txtCollCartageRemark.Text),
                                        strBookingDetailsData, txtSenderEmailID.Text, txtReceiverEmailID.Text, (blnIsOfflineMode ? 1 : 0),
                                        strOfflineVehicle, txtSenderGSTN.Text, txtReceiverGSTN.Text,
                                        intIDProofTypeID, strIDProofNo, strsenderimgurl, strIDProofimgurl, intOfflineVehicleID,
                                        Common.GetBranchCityID(), Common.GetBranchID(),
                                        strAllConsignmentSubTypeID, strAllDescription, strAllQuantity, strAllGoodsValue, strAllVolume,
                                        strAllActualWeight, strAllChargedWeight, strAllActualRate, strAllRate, strAllFreight, strAllAmount,
                                        strAllCurrentCity, strAllCurrentBranch, strAllMOPID, _intPickupCityID, _intDropOffCityID, EwayBillNo,_PickupCartage,
                                        _CommCartage,_ReturnCartage,Common.GetUserID(), ref err);

                        if (err == "" && dt != null && dt.Rows.Count > 0)
                        {
                            try
                            {
                                intBookingIDtoValidate = Convert.ToInt32(dt.Rows[0]["BookingID"].ToString());
                                strLRNOtoValidate = dt.Rows[0]["LRNo"].ToString();
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
                                //chkCrossing.Checked = false;

                                if (is_stax_company && is_GSTN_branch == 1)
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

                                RemoveValuesFromControls(blnIsOfflineMode);

                                lblLRNo.Visible = true;
                                lblTotalAmtCnf.Visible = true;

                                string strLRNO = dt.Rows[0]["LRNo"].ToString();


                                strLRNO = Common.FormattedLR(strLRNO);

                                lblLRNo.Text = "LRN : " + strLRNO + "  Sender : " + dt.Rows[0]["Sender"].ToString()
                                                    + "  Receiver : " + dt.Rows[0]["RecName"].ToString()
                                                    + (txtFreightChg.Visible == true ? "  Freight : " + dt.Rows[0]["Freight"].ToString() : "")
                                                    + (txtDeliveryChg.Visible == true ? "  Delivery chg : " + dt.Rows[0]["DeliveryCharges"].ToString() : "")
                                    //+ (txtCollectionChg.Visible == true ? "  Collection chg : " + dt.Rows[0]["CollectionCharges"].ToString() : "")
                                                    + (txtCartage.Visible == true ? "  Cartage : " + dt.Rows[0]["CartageAmount"].ToString() : "")
                                                    + (txtDocumentChg.Visible == true ? "  Document : " + dt.Rows[0]["DocumentCharges"].ToString() : "")


                                                    + (lblInsuranceBx.Text == "Door-Del.Chg" ? "  Door-Del.Chg : " + dt.Rows[0]["DoorDelCharges"].ToString() : "  Insurance : " + dt.Rows[0]["Insurance"].ToString())
                                    //+ (txtInsurance.Visible == true ? "  Insurance : " + dt.Rows[0]["Insurance"].ToString() : "")

                                                    + (lblServiceTxBx.Text == "Hamali Chg" ? "  Hamali Chg : " + dt.Rows[0]["HamaliCharges"].ToString() : "  Service tax : " + (Convert.ToDouble("0" + dt.Rows[0]["ServiceTaxAmount"].ToString()) + Convert.ToDouble("0" + dt.Rows[0]["ServiceTaxAmountCartage"].ToString())).ToString());
                                //+ (txtServiceTax.Visible == true ? "  Service tax : " + dt.Rows[0]["ServiceTaxAmount"].ToString() : "")                                                            

                                lblTotalAmtCnf.Text = "Total : " + dt.Rows[0]["NetAmount"].ToString();
                                lblLastLRNo.Text = strLRNO; //  dt.Rows[0]["LRNo"].ToString();
                                timerGetLastLR.Stop(); timerGetLastLR.Start();

                                UpdateCreditLimit();

                                dlgUploadImgtos3 TG = new dlgUploadImgtos3(UploadImgtoS3);
                                TG.BeginInvoke(null, null);


                                if (!blnIsOfflineMode)
                                {
                                    DialogResult dr1 = MessageBox.Show("Do you want to take a print?", "Print", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                                    if (dr1 == DialogResult.Yes)
                                    {
                                        int _intBookingID = Convert.ToInt32(dt.Rows[0]["BookingID"]);
                                        string _StrLRNO = dt.Rows[0]["LRNo"].ToString();
                                        DataTable dtBooking = App_Code.Cargo.CargoBookingDetailsToPrint(_intBookingID, intBranchID, intUserID, _StrLRNO);

                                        if (dtBooking != null && dtBooking.Rows.Count > 0)
                                        {

                                            if (Convert.ToInt32(dtBooking.Rows[0]["CargoHasBilltyLaserPrint"]) == 1)
                                            {
                                                App_Code.Common.print_Laser_billty(_intBookingID, dtBooking.Rows[0]["BilltyTemplate"].ToString());

                                                if (blnIsSTACargoCompany && (intCompanyID == 406 || intCompanyID == 1 || intCompanyID == 3035)) // STA
                                                    Print_Sticker(dtBooking, _intBookingID, dt.Rows[0]["FormattedLRNo"].ToString());
                                            }
                                            else
                                            {
                                                if (blnIsSTACargoCompany && (intCompanyID == 406 || intCompanyID == 1 || intCompanyID == 3035)) // STA
                                                {
                                                    App_Code.Common.print_DM_STA(false, dtBooking, intCompanyID, intBranchID, intUserID); //DOT-MATRIX PRINT FOR STA

                                                    Print_Sticker(dtBooking, _intBookingID, dt.Rows[0]["FormattedLRNo"].ToString());
                                                }
                                                else
                                                {
                                                    Cargo.frmPrintTicket frmPrint = new Cargo.frmPrintTicket(Convert.ToInt32(dt.Rows[0]["BookingID"]), intBranchID, intUserID,
                                                                                dt.Rows[0]["LRNo"].ToString(), dtBooking, false, strLaserPrinterName, strStickerPrinterName);
                                                    frmPrint.ShowDialog();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            MessageBox.Show("No data found to print.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }

                                    }
                                }
                                radDDDestCity.Focus();
                            }
                        }
                        else
                        {
                            MessageBox.Show(err, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            if (err.Contains("You can not book others Blocked LR or Remove LR from search box."))
                            {
                                txtLRNo.Text = "";
                            }
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

        public void Print_Sticker(DataTable dtBooking, int intBookingID, string FormattedLrNo)
        {
            DialogResult dr2 = MessageBox.Show("Do you want to take a sticker print?", "Sticker Print", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr2 == DialogResult.Yes)
            {
                int totalunit = Convert.ToInt32(dtBooking.Rows[0]["NoA"].ToString());

                Cargo.frmPrintTicket frmPrint = new Cargo.frmPrintTicket(intBookingID, intBranchID, intUserID,
                                        FormattedLrNo, dtBooking, true, strLaserPrinterName, strStickerPrinterName, "", totalunit);
                frmPrint.ShowDialog();
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
            int PTypeID = Convert.ToInt32(radDPayType.SelectedValue);

            if (intCompanyID == 1 || intCompanyID == 406)
            {
                if (intisBkgCreditLimit == 0 && radDPayType.SelectedValue.ToString() == "5") // On Account
                {
                    MessageBox.Show("You cannot do On-Account booking with selected party, Please enable Booking Credit Limit for party.", "Ooops..", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    radDPartySender.Focus();
                    return false;
                }
            }

            if (intCompanyID != 406 && blnIsOfflineMode == true && radDDOfflineBookingUser.Items.Count == 0)
            {
                MessageBox.Show("Please Select User For Offline Booking.");
                radDDOfflineBookingUser.Focus();
                return false;
            }

            if (blnIsOfflineMode == true && txtManualLR.Text == "")
            {
                MessageBox.Show("Please Enter Manual LRNo for Offline Booking.");
                txtManualLR.Focus();
                return false;
            }

            if (radDPayType.SelectedValue.ToString() == "6" || radDPayType.SelectedValue.ToString() == "7")
            {
                if (txtManualLR.Text.ToString().Trim() == "")
                {
                    MessageBox.Show("Please enter Manual LR No.");
                    txtManualLR.Focus();
                    return false;
                }
            }

            if (blnIsOfflineMode == true && (radDVehicleNos.SelectedItem.Text.ToString() == null || radDVehicleNos.SelectedItem.Text.ToString() == "--Select--"))
            //(radDVehicleNos.SelectedValue == null || radDVehicleNos.SelectedValue.ToString() == "--Select--"))
            {
                MessageBox.Show("Please Select Vehicle for Offline.");
                radDVehicleNos.Focus();
                return false;
            }

            if (radDDBookingCity.Items.Count == 0)
            {
                MessageBox.Show("Please select Booking City.");
                radDDBookingCity.Focus();
                return false;
            }

            if (blnIsOfflineMode == false && Convert.ToInt32(radDDBookingCity.SelectedValue) != intBranchCityID)
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

            if (blnIsOfflineMode == false && Convert.ToInt32(radDDBookingBranch.SelectedValue) != intBranchID)
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

            if (radDDDestBranch.Items.Count == 0 || radDDDestBranch.SelectedValue.ToString() == "0")
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

            if (chkFTLAssignCities.Visible && chkFTLAssignCities.Checked)
            {
                if (_intPickupCityID == 0 || _intDropOffCityID == 0)
                {
                    if (_intPickupCityID == 0)
                    {
                        MessageBox.Show("Please select Pickup City for FTL type booking.");
                        chkFTLAssignCities.Focus();
                        return false;
                    }

                    if (_intDropOffCityID == 0)
                    {
                        MessageBox.Show("Please select DropOff City for FTL type booking.");
                        chkFTLAssignCities.Focus();
                        return false;
                    }
                }
                else if ((PTypeID == 2 || PTypeID == 7) && _intDropOffCityID != Convert.ToInt32(radDDDestCity.SelectedValue))
                {
                    MessageBox.Show("DropOff City is other than destination city for FTL type To-Pay booking.");
                    chkFTLAssignCities.Focus();
                    return false;
                }
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

            if (txtMobileNoReceiver.Text == "")
            {
                txtMobileNoReceiver.Text = "0";
            }

            if (txtMobileNo.Text == "")
            {
                txtMobileNo.Text = "0";
            }

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

                    if (chkFTLAssignCities.Visible && chkFTLAssignCities.Checked)
                    {
                        if (ConsignmentSubType != strFTLTypeConsID)
                        {
                            MessageBox.Show("Selected ConsignmentType is not FTL type.");
                            radGridConsignItems.Rows[i].Cells["ConsignmentType"].BeginEdit();
                            return false;
                        }
                    }
                    else if ((ConsignmentSubType == strFTLTypeConsID) && !chkFTLAssignCities.Checked)
                    {
                            MessageBox.Show("Pickup/DropOff cities are not selected for FTL type consignment.");
                            chkFTLAssignCities.Focus();
                            return false;
                    }

                    if (ConsignmentSubType != "0" || Description != "" || (Qty != "" && Qty != "0") || (Goodsvalue != "" && Goodsvalue != "0") || (Rate != "" && Rate != "0") || radGridConsignItems.Rows.Count == 1)
                    {
                        if (ConsignmentSubType == "0")
                        {
                            MessageBox.Show("Please select ConsignmentType");
                            radGridConsignItems.Rows[i].Cells["ConsignmentType"].BeginEdit();
                            return false;
                        }

                        int quantity = new int();
                        bool validqty = Int32.TryParse(Qty, out quantity);

                        if (validqty)
                        {
                            if (quantity <= 0)
                            {
                                MessageBox.Show("Please Enter Unit.");
                                radGridConsignItems.Rows[i].Cells["Qty"].BeginEdit();
                                return false;
                            }
                        }
                        else
                        {
                            MessageBox.Show("Please Enter valid Unit.");
                            radGridConsignItems.Rows[i].Cells["Qty"].BeginEdit();
                            return false;
                        }

                        double dblRate = new double();
                        bool validRate = double.TryParse(Rate, out dblRate);

                        if (validRate)
                        {
                            if (dblRate <= 0 && radDPayType.SelectedValue.ToString() != "3" && !is_direct_freight_company)
                            {
                                MessageBox.Show("Please Enter Rate.");
                                radGridConsignItems.Rows[i].Cells["Rate"].BeginEdit();
                                return false;
                            }
                        }
                        else
                        {
                            MessageBox.Show("Please Enter Valid Rate.");
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

            if (intIsEWayBillNo == 1 && txtEwayBillNo.Visible)
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
                if (total_freight > dcmMinTaxAmount) //750)
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
                    if (Convert.ToInt32(txtAmount.Text) != 0)
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

            if (is_stax_company && (is_GSTN_branch == 1) && txtServiceTax.Text.ToString() != "" && txtServiceTax.Text.ToString() != "0")
            {
                if (radDDCStaxPaidBy.SelectedValue.ToString() != "2")
                {
                    MessageBox.Show("ServiceTax will be applicable only if Service Paid By Transporter");
                    return false;
                }
            }
            return true;
        }

        public void RemoveValuesFromControls(bool isOffline = false)
        {
            try
            {
                //chkCrossing.Checked = false;
                radDDBookingCity.Enabled = isOffline;
                radDDBookingBranch.Enabled = isOffline;

                radDDOfflineBookingUser.Enabled = isOffline;

                txtManualLR.Text = "";

                if (isOffline)
                {
                    //  radDDDestCity.SelectedValue = intBranchCityID.ToString();
                    //  radDDDestBranch.SelectedValue = intBranchID.ToString();
                    // radDDBookingCity.SelectedIndex = 0;
                }
                else
                {
                    radDDBookingCity.SelectedValue = intBranchCityID.ToString();
                    radDDBookingBranch.SelectedValue = intBranchID.ToString();
                    radDDDestCity.SelectedIndex = 0;
                }

                radDVehicleNos.SelectedIndex = 0;

                intShownBookingID = 0;
                radDPayType.SelectedIndex = 0;
                chkIsPartySender.Checked = false;
                chkIsPartyReceiver.Checked = false;
                txtNameSender.Text = "";
                txtNameReceiver.Text = "";
                txtMobileNo.Text = "";
                txtMobileNoReceiver.Text = "";
                txtCartageDel.Text = "0";
                txtCollCartageRemark.Text = "";
                txtSenderGSTN.Text = "";
                txtReceiverGSTN.Text = "";
                //txtAddressReceiver.Text = "";

                chkRecMobileGetData.Checked = false;
                chkSenderMobileGetData.Checked = false;

                for (int i = dtGriSourse.Rows.Count - 1; i >= 0; i--)
                {
                    dtGriSourse.Rows.RemoveAt(i);
                }

                AddNewRow("0", "0", "", "0", "0", "0", "0", 0, 0, 0, 0, 0, 0);

                radGridConsignItems.DataSource = dtGriSourse;
                radGridConsignItems.Enabled = true;

                txtFreightChg.Text = "0";

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

                if (blnIsOfflineMode)
                {
                    if (intCompanyID != 805 && intCompanyID != 2921)
                    {
                        radDDDestCity.Enabled = true;
                        radDDDestBranch.Enabled = true;
                    }
                }

                radDPayType.Enabled = true;

                SetPartyControls();

                txtNameSender.ReadOnly = false;
                txtMobileNo.ReadOnly = false;
                txtNameReceiver.ReadOnly = false;
                txtMobileNoReceiver.ReadOnly = false;

                txtDeliveryChg.ReadOnly = false;
                txtCartage.ReadOnly = false;
                txtDocumentChg.ReadOnly = false;
                txtInsurance.ReadOnly = false;

                txtComment.ReadOnly = false;

                radDModeofPayment.Enabled = false;
                radGridConsignItems.Enabled = true;
                radBSave.Enabled = true;
                radBSave.Text = "Save";

                txtBillNo.Text = "";
                txtBillNo.ReadOnly = false;
                dtBillNo.Enabled = false;
                //radDDDDBill.Enabled = true;
                //radDDMMBill.Enabled = true;
                //radDDYYBill.Enabled = true;

                txtEwayBillNo.Text = "";
                txtEwayBillNo.ReadOnly = false;
                radbtnEwayBillDate.Enabled = true;

                txtSenderEmailID.Text = "";
                txtReceiverEmailID.Text = "";

                fillDates();
                txtLRNo.Text = "";

                txtSenderGSTN.Text = "";
                txtReceiverGSTN.Text = "";

                intIDProofTypeID = 0;
                strIDProofNo = "";
                //strSenderImageName = "";
                //strIDProofImageName = "";

                lnkimageandID.Text = "Capture Image and ID";

                dcmLength = 0;
                dcmWidth = 0;
                dcmHeight = 0;
                dcmVolumetricWeight = 0;

                if (intHasDeliveryType == 1)
                {
                    lblDeliveryType.Visible = true;
                    raddDeliveryType.Visible = true;
                }
                else
                {
                    lblDeliveryType.Visible = false;
                    raddDeliveryType.Visible = false;
                }

                ResetFTLChanges();
                ResetCartageBreakups();
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

            FillBookingBranch();
            radDDBookingBranch.SelectedValue = intBranchID.ToString();

            if (radBOfflineBooking.Text == "Online Booking")
            {
                radBOfflineBooking_Click(null, null);
            }
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
            double dblCollectionChg = 0;// Convert.ToDouble((txtCollectionChg.Text != "" ? txtCollectionChg.Text : "0"));
            double dblCartage = Convert.ToDouble((txtCartage.Text != "" ? txtCartage.Text : "0"));
            double dblCartageDel = Convert.ToDouble((txtCartageDel.Text != "" ? txtCartageDel.Text : "0"));
            double dblHamaliChg = Convert.ToDouble((txtHamaliChg.Text != "" ? txtHamaliChg.Text : "0"));

            double dblTotalAmt = 0;

            if (intCompanyID != 0)
            {
                if (is_stax_company)
                {
                    //if (dblFreightChg > Convert.ToDouble(dcmMinTaxAmount)) // 750)
                    //{
                    dblServiceTax = 0;
                    txtServiceTax.Text = dblServiceTax.ToString();
                    //}

                    dblTotalAmt = dblDeliveryChg + dblFreightChg + dblDocumentChg + dblInsurance + dblCollectionChg + dblCartage + dblCartageDel + dblHamaliChg;

                    if (dblTotalAmt >= Convert.ToDouble(dcmMinTaxAmount)) //750)
                    {
                        if (!blnIsOfflineMode || (blnIsOfflineMode && Common.CargoHasSTaxOfflineBooking()))
                        {
                            if (is_stax_company && (is_GSTN_branch == 1) && radDDCStaxPaidBy.Visible && radDDCStaxPaidBy.SelectedValue.ToString() == "2")
                            {
                                /********** Pradeep : 20180-03-27 :: Mahesh Cargo Requirement to calculate service tax on freight, not in total amount   *******/
                                if (intCompanyID == 2891)
                                    dblServiceTax = dblFreightChg * Convert.ToDouble(dcmTaxPCT) / 100.00;
                                else if((intCompanyID != 3034) || (intCompanyID == 3034 && txtBillNo.Visible && txtBillNo.Text.ToString().Length > 0))
                                    dblServiceTax = dblTotalAmt * Convert.ToDouble(dcmTaxPCT) / 100.00;
                            }
                            if (blnIsSTACargoCompany)
                            {
                                dblServiceTax = Math.Round(dblServiceTax);
                                txtServiceTax.Text = Math.Round(dblServiceTax).ToString();
                            }
                            else
                                txtServiceTax.Text = Math.Round(dblServiceTax, 2).ToString();
                        }
                    }
                }

                dblTotalAmt = dblDeliveryChg + dblFreightChg + dblDocumentChg + dblServiceTax + dblInsurance + dblCollectionChg + dblCartage + dblCartageDel + dblHamaliChg;
                txtAmount.Text = Math.Round(dblTotalAmt, 2).ToString();
            }
            else
            {
                // KTN : Not Going Here, Need to check why this has been written
                dblTotalAmt = dblDeliveryChg + dblFreightChg + dblDocumentChg + dblInsurance + dblCollectionChg + dblCartage + dblCartageDel + dblHamaliChg;

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

        }

        public void setPaymentMode()
        {
            try
            {
                radDModeofPayment.SelectedIndex = 0;

                //if (!blnIsSTACargoCompany)
                // {
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
                }
                                    
                if (blnIsSTACargoCompany)
                    radDModeofPayment.Enabled = false;
                // }
            }
            catch (Exception ex)
            {

            }
        }
        //dispatch details
        private void radButton10_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            tableLayoutPanel1.Enabled = false;
            if (!blnIsOfflineMode)
                tableLayoutPanel1.BackColor = Color.Silver;
            Cargo.frmDispatch frm = new Cargo.frmDispatch();
            frm.ShowDialog();
            this.Cursor = Cursors.Default;
            if (!blnIsOfflineMode)
                tableLayoutPanel1.BackColor = Color.White;
            tableLayoutPanel1.Enabled = true;
        }

        private void radButton11_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            tableLayoutPanel1.Enabled = false;
            if (!blnIsOfflineMode)
                tableLayoutPanel1.BackColor = Color.Silver;
            Cargo.frmReceipt frm = new Cargo.frmReceipt(intReceiveType);
            frm.ShowDialog();
            this.Cursor = Cursors.Default;
            if (!blnIsOfflineMode)
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
                    //MessageBox.Show("Please enter valid LRNo.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //txtLRNo.Focus();
                    //return;

                    this.Cursor = Cursors.WaitCursor;
                    tableLayoutPanel1.Enabled = false;
                    if (!blnIsOfflineMode)
                        tableLayoutPanel1.BackColor = Color.Silver;

                    Cargo.frmDelivery frm = new Cargo.frmDelivery(intBranchID, intUserID, intBranchCityID, intCompanyID);
                    //frm.Width = this.Width - 200;
                    //frm.Height = this.Height - 200;
                    frm.ShowDialog();

                    txtLRNo.Text = "";
                    txtLRNo.Focus();

                    this.Cursor = Cursors.Default;

                    if (!blnIsOfflineMode)
                        tableLayoutPanel1.BackColor = Color.White;
                    tableLayoutPanel1.Enabled = true;
                }
                else
                {
                    string strLRNO = txtLRNo.Text;

                    if (blnIsSTACargoCompany)// && strSelectedLRNo == "")
                    {
                        if (!strLRNO.Contains("/"))
                        {
                            if (!strLRNO.Contains("\\"))
                            {
                                strLRNO = "0000000" + strLRNO;
                                strLRNO = strLRNO.Substring(strLRNO.Length - 7);

                                //strLRNO = Common.GetCacheLoginCode() + "\\" + strLRNO;

                                DataSet dsTemp = App_Code.Cargo.GetLRDetailsForPopUp(strLRNO, 0, intCompanyID, intBranchID, intUserID, intBranchCityID, 0);

                                Cargo.frmLRDetailsPopUp frm = new Cargo.frmLRDetailsPopUp(this, dsTemp);
                                frm.Width = Convert.ToInt32(this.Width * 0.95);
                                frm.Height = Convert.ToInt32(this.Height * 0.90);
                                frm.ShowDialog();

                                strLRNO = strSelectedLRNo;

                            }
                        }
                    }

                    this.Cursor = Cursors.WaitCursor;
                    if (strLRNO != "")
                    {
                        DataSet ds = new DataSet();

                        ds = App_Code.Cargo.GetLuggageForDelivery(strLRNO, intBranchCityID, intBranchID, -1, 1, intUserID);

                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            int intDestinationCityID = Convert.ToInt32(ds.Tables[0].Rows[0]["DestinationCityID"].ToString());

                            if (intDestinationCityID == intBranchCityID)
                            {
                                this.Cursor = Cursors.WaitCursor;
                                tableLayoutPanel1.Enabled = false;
                                if (!blnIsOfflineMode)
                                    tableLayoutPanel1.BackColor = Color.Silver;

                                Cargo.frmDelivery frm = new Cargo.frmDelivery(strLRNO, intBranchID, intUserID, intBranchCityID, intCompanyID, ds, -1);
                                //frm.Width = this.Width - 200;
                                //frm.Height = this.Height - 200;
                                frm.ShowDialog();

                                txtLRNo.Text = "";
                                txtLRNo.Focus();

                                this.Cursor = Cursors.Default;

                                if (!blnIsOfflineMode)
                                    tableLayoutPanel1.BackColor = Color.White;

                                tableLayoutPanel1.Enabled = true;
                            }
                            else
                            {
                                MessageBox.Show("You can not deliver other branch's LRN.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        else
                        {
                            MessageBox.Show("No data found for given LRN.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
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

                string strLRNO = txtLRNo.Text;

                if (blnIsSTACargoCompany)
                {
                    if (!strLRNO.Contains("/"))
                    {
                        if (!strLRNO.Contains("\\"))
                        {
                            strLRNO = "0000000" + strLRNO;
                            strLRNO = strLRNO.Substring(strLRNO.Length - 7);

                            //strLRNO = Common.GetCacheLoginCode() + "\\" + strLRNO;

                            DataSet dsTemp = App_Code.Cargo.GetLRDetailsForPopUp(strLRNO, 0, intCompanyID, intBranchID, intUserID, intBranchCityID, 0);

                            Cargo.frmLRDetailsPopUp frm = new Cargo.frmLRDetailsPopUp(this, dsTemp);
                            frm.Width = Convert.ToInt32(this.Width * 0.95);
                            frm.Height = Convert.ToInt32(this.Height * 0.90);
                            frm.ShowDialog();

                            strLRNO = strSelectedLRNo;

                        }
                    }
                }
                //if (!strLRNO.Contains("/"))
                //{
                //    if (!strLRNO.Contains("\\"))
                //    {
                //        strLRNO = Common.GetCacheLoginCode() + "\\" + strLRNO;
                //    }
                //}

                this.Cursor = Cursors.WaitCursor;
                if (strLRNO != "")
                {
                    DataSet ds = new DataSet();

                    ds = App_Code.Cargo.GetLuggageForRefund(intBranchCityID, intBranchID, strLRNO, intUserID);

                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        this.Cursor = Cursors.WaitCursor;
                        tableLayoutPanel1.Enabled = false;
                        if (!blnIsOfflineMode)
                            tableLayoutPanel1.BackColor = Color.Silver;

                        Cargo.frmRefund frm = new Cargo.frmRefund(ds);
                        //frm.Width = this.Width - 200;
                        //frm.Height = this.Height - 200;
                        frm.ShowDialog();

                        txtLRNo.Text = "";
                        txtLRNo.Focus();

                        this.Cursor = Cursors.Default;

                        if (!blnIsOfflineMode)
                            tableLayoutPanel1.BackColor = Color.White;
                        tableLayoutPanel1.Enabled = true;
                    }
                    else
                    {
                        MessageBox.Show("No data found for given LRN.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        //Status changed
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

                string strLRNO = txtLRNo.Text;

                if (blnIsSTACargoCompany)
                {
                    if (!strLRNO.Contains("/"))
                    {
                        if (!strLRNO.Contains("\\"))
                        {
                            strLRNO = "0000000" + strLRNO;
                            strLRNO = strLRNO.Substring(strLRNO.Length - 7);

                            //strLRNO = Common.GetCacheLoginCode() + "\\" + strLRNO;

                            DataSet dsTemp = App_Code.Cargo.GetLRDetailsForPopUp(strLRNO, 0, intCompanyID, intBranchID, intUserID, intBranchCityID, 0);

                            Cargo.frmLRDetailsPopUp frm = new Cargo.frmLRDetailsPopUp(this, dsTemp);
                            frm.Width = Convert.ToInt32(this.Width * 0.95);
                            frm.Height = Convert.ToInt32(this.Height * 0.90);
                            frm.ShowDialog();

                            strLRNO = strSelectedLRNo;

                        }
                    }
                }

                //if (!strLRNO.Contains("/"))
                //{
                //    if (!strLRNO.Contains("\\"))
                //    {
                //        strLRNO = Common.GetCacheLoginCode() + "\\" + strLRNO;
                //    }
                //}

                this.Cursor = Cursors.WaitCursor;
                if (strLRNO != "")
                {
                    DataSet ds = new DataSet();

                    ds = App_Code.Cargo.GetStatus(intCompanyID, intBranchID, strLRNO, intUserID);

                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        this.Cursor = Cursors.WaitCursor;
                        tableLayoutPanel1.Enabled = false;
                        if (!blnIsOfflineMode)
                            tableLayoutPanel1.BackColor = Color.Silver;

                        Cargo.frmStatus frm = new Cargo.frmStatus(strLRNO, ds);
                        //frm.Width = this.Width - 200;
                        //frm.Height = this.Height - 200;
                        frm.ShowDialog();

                        txtLRNo.Text = "";
                        txtLRNo.Focus();
                        this.Cursor = Cursors.Default;

                        if (!blnIsOfflineMode)
                            tableLayoutPanel1.BackColor = Color.White;
                        tableLayoutPanel1.Enabled = true;
                    }
                    else
                    {
                        MessageBox.Show("No data found for given LRN.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

                string strLRNO = txtLRNo.Text;


                int _intBookingID = 0;
                string _StrLRNO = strLRNO; // txtLRNo.Text;
                DataTable dtBooking = null;

                if (blnIsSTACargoCompany)
                {
                    if (!strLRNO.Contains("/"))
                    {
                        if (!strLRNO.Contains("\\"))
                        {
                            strLRNO = "0000000" + strLRNO;
                            strLRNO = strLRNO.Substring(strLRNO.Length - 7);

                            _StrLRNO = strLRNO;
                            //strLRNO = Common.GetCacheLoginCode() + "\\" + strLRNO;

                            DataSet dsTemp = App_Code.Cargo.GetLRDetailsForPopUp(_StrLRNO, _intBookingID, intCompanyID, intBranchID, intUserID, intBranchCityID, 0);

                            Cargo.frmLRDetailsPopUp frm = new Cargo.frmLRDetailsPopUp(this, dsTemp);
                            frm.Width = Convert.ToInt32(this.Width * 0.95);
                            frm.Height = Convert.ToInt32(this.Height * 0.90);
                            frm.ShowDialog();

                            _StrLRNO = strSelectedLRNo;

                        }
                    }
                }

                this.Cursor = Cursors.WaitCursor;
                if (_StrLRNO != "")
                {

                    dtBooking = App_Code.Cargo.CargoBookingDetailsToPrint(_intBookingID, intBranchID, intUserID, _StrLRNO);


                    if (dtBooking != null && dtBooking.Rows.Count > 0)
                    {
                        if (Convert.ToInt32(dtBooking.Rows[0]["CargoHasBilltyLaserPrint"]) == 1 && dtBooking.Rows[0]["BilltyTemplate"].ToString().Trim() != "")
                        {
                            string path = "C:\\Windows\\Temp\\Billty";
                            bool exists = System.IO.Directory.Exists(path);
                            if (!exists)
                            {
                                System.IO.Directory.CreateDirectory(path);
                            }
                            path += "\\" + dtBooking.Rows[0]["BookingID"] + ".html";
                            TextWriter tw = new StreamWriter(path);
                            tw.Write(dtBooking.Rows[0]["BilltyTemplate"].ToString());

                            tw.Close();

                            System.Diagnostics.Process.Start("C:\\Windows\\Temp\\Billty\\" + dtBooking.Rows[0]["BookingID"] + ".html");

                            if (blnIsSTACargoCompany && intCompanyID == 406)
                                ReprintSticker(dtBooking, _intBookingID, _StrLRNO);
                        }
                        else
                        {
                            if (blnIsSTACargoCompany && (intCompanyID == 406 || intCompanyID == 3035 || intCompanyID == 1))  // STA
                            {
                                DialogResult dr1 = MessageBox.Show("Do you want to take a Bilty print?", "Bilty Print", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                if (dr1 == DialogResult.Yes)
                                    App_Code.Common.print_DM_STA(false, dtBooking, intCompanyID, intBranchID, intUserID);

                                ReprintSticker(dtBooking, _intBookingID, _StrLRNO);
                            }
                            else
                            {
                                Cargo.frmPrintTicket frmPrint = new Cargo.frmPrintTicket(_intBookingID, intBranchID, intUserID,
                                                       _StrLRNO, dtBooking, false, strLaserPrinterName, strStickerPrinterName);
                                frmPrint.ShowDialog();
                            }
                        }

                        txtLRNo.Text = "";
                        txtLRNo.Focus();
                    }
                    else
                    {
                        MessageBox.Show("No data found to print.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    if (!blnIsOfflineMode)
                        tableLayoutPanel1.BackColor = Color.Silver;

                    Cargo.frmDeliveryMemo frm = new Cargo.frmDeliveryMemo(ds, this);
                    frm.Width = Convert.ToInt32(this.Width * 0.98);
                    frm.Height = Convert.ToInt32(this.Height * 0.90);
                    frm.ShowDialog();

                    //txtLRNo.Text = "";

                    //if (strSelectedLRNo != "")
                    //{
                    //    txtLRNo.Text = strSelectedLRNo;
                    //    radBDelivery_Click(null, null);
                    //}

                    txtLRNo.Focus();

                    this.Cursor = Cursors.Default;

                    if (!blnIsOfflineMode)
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

        private void frmLuggageBookingV2_Enter(object sender, EventArgs e)
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

                string strLRNO = txtLRNo.Text;

                if (blnIsSTACargoCompany)
                {
                    if (!strLRNO.Contains("/"))
                    {
                        if (!strLRNO.Contains("\\"))
                        {
                            strLRNO = "0000000" + strLRNO;
                            strLRNO = strLRNO.Substring(strLRNO.Length - 7);

                            //strLRNO = Common.GetCacheLoginCode() + "\\" + strLRNO;

                            DataSet dsTemp = App_Code.Cargo.GetLRDetailsForPopUp(strLRNO, 0, intCompanyID, intBranchID, intUserID, intBranchCityID, 0);

                            Cargo.frmLRDetailsPopUp frm = new Cargo.frmLRDetailsPopUp(this, dsTemp);
                            frm.Width = Convert.ToInt32(this.Width * 0.95);
                            frm.Height = Convert.ToInt32(this.Height * 0.90);
                            frm.ShowDialog();

                            strLRNO = strSelectedLRNo;

                        }
                    }
                }
                //if (!strLRNO.Contains("/"))
                //{
                //    if (!strLRNO.Contains("\\"))
                //    {
                //        strLRNO = Common.GetCacheLoginCode() + "\\" + strLRNO;
                //    }
                //}

                this.Cursor = Cursors.WaitCursor;
                if (strLRNO != "")
                {
                    DataSet dsBooking = App_Code.Cargo.GetRptLRSearchResults(strLRNO, intBranchID, intCompanyID, DateTime.Now, DateTime.Now, intUserID);

                    if (dsBooking != null && dsBooking.Tables.Count > 0 && dsBooking.Tables[0].Rows.Count > 0)
                    {
                        if (!chkShowType.Checked)
                        {
                            if (dsBooking.Tables.Count > 1 && dsBooking.Tables[1].Rows.Count > 0)
                                ShowLRNo(dsBooking);
                            else
                            {
                                if (dsBooking.Tables[0].Columns.Contains("BlockedId"))
                                {
                                    if (Convert.ToInt32(dsBooking.Tables[0].Rows[0]["IsAvailable"].ToString()) == 1)
                                    {
                                        chkIsPartySender.Checked = true;
                                        //radDPartySender.selectb SelectedText = dsBooking.Tables[0].Rows[0]["PartyName"].ToString();
                                        radDPartySender.SelectedIndex = radDPartySender.DropDownListElement.FindStringExact(dsBooking.Tables[0].Rows[0]["PartyName"].ToString());

                                        radDPartySender.Enabled = false;
                                        lblLRNo.Visible = true;
                                        lblTotalAmtCnf.Visible = false;
                                        lblLRNo.Text = "Next Booking On Blocked LRNO : " + txtLRNo.Text;
                                    }
                                    else
                                    {
                                        lblLRNo.Visible = false;
                                        lblTotalAmtCnf.Visible = false;
                                        txtLRNo.Text = "";
                                        MessageBox.Show("LR is already consumed / released.", "Information", MessageBoxButtons.OK);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (dsBooking.Tables[0].Columns.Contains("BlockedId"))
                            {
                                MessageBox.Show("You can't view blocked LRNo.", "Information", MessageBoxButtons.OK);
                            }
                            else
                            {
                                Cargo.frmReportView frmRpt = new Cargo.frmReportView("Show", dsBooking);

                                frmRpt.ShowDialog();
                            }
                        }
                        //txtLRNo.Text = "";
                        txtLRNo.Focus();
                    }
                    else
                    {
                        MessageBox.Show("No data found for give LR.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                txtCartageDel.Text = dsBkgs.Tables[0].Rows[0]["CartageDeliveryAmount"].ToString();
                txtCartage.Text = dsBkgs.Tables[0].Rows[0]["CartageAmount"].ToString();
                txtDocumentChg.Text = dsBkgs.Tables[0].Rows[0]["DocumentCharges"].ToString();
                txtInsurance.Text = dsBkgs.Tables[0].Rows[0]["Insurance"].ToString();
                txtServiceTax.Text = dsBkgs.Tables[0].Rows[0]["ServiceTaxAmount"].ToString();
                txtCollCartageRemark.Text = dsBkgs.Tables[0].Rows[0]["CartageRemark"].ToString();

                radDModeofPayment.SelectedValue = dsBkgs.Tables[0].Rows[0]["ModeOfPayment"].ToString();

                txtSenderAddress.Text = dsBkgs.Tables[0].Rows[0]["SenderAddress"].ToString();

                string PayBillNo = dsBkgs.Tables[0].Rows[0]["PartyBillNo"].ToString();

                if (PayBillNo != "" && PayBillNo != "00")
                {
                    string[] strPayBillDet = PayBillNo.Split('~');
                    txtBillNo.Text = strPayBillDet[0].Trim();

                    DateTime dtBill = DateTime.ParseExact(strPayBillDet[1], "dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
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
                    
                    DateTime dtEwayBillEnd = Common.GetServerTime(0,0);

                    if(strEwayBillDet.Length > 2)
                        dtEwayBillEnd = DateTime.ParseExact(strEwayBillDet[2], "dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture);

                    dtEwayBillStartDate = dtEwayBillStart;
                    dtEwayBillEndDate = dtEwayBillEnd;
                    txtEwayBillNo.ReadOnly = true;
                }

                txtComment.Text = dsBkgs.Tables[0].Rows[0]["Remarks"].ToString();
                txtSenderGSTN.Text = dsBkgs.Tables[0].Rows[0]["SenderGSTN"].ToString();
                txtReceiverGSTN.Text = dsBkgs.Tables[0].Rows[0]["ReceiverGSTN"].ToString();
                txtHamaliChg.Text = dsBkgs.Tables[0].Rows[0]["HamaliCharges"].ToString();

                if (dsBkgs.Tables[0].Rows[0]["ServiceTaxPaidBy"].ToString().ToLower() == "tr")
                    radDDCStaxPaidBy.SelectedValue = 2;
                else if (dsBkgs.Tables[0].Rows[0]["ServiceTaxPaidBy"].ToString().ToLower() == "cr")
                    radDDCStaxPaidBy.SelectedValue = 3;
                else if (dsBkgs.Tables[0].Rows[0]["ServiceTaxPaidBy"].ToString().ToLower() == "cn")
                    radDDCStaxPaidBy.SelectedValue = 4;
                else
                    radDDCStaxPaidBy.SelectedValue = 1;

                createDT();

                if (Convert.ToInt32(dsBkgs.Tables[0].Rows[0]["PickupCityID"]) > 0 && Convert.ToInt32(dsBkgs.Tables[0].Rows[0]["DropOffCityID"]) > 0)
                {

                    _intPickupCityID = Convert.ToInt32(dsBkgs.Tables[0].Rows[0]["PickupCityID"]);
                    _pickupCityShortName = dsBkgs.Tables[0].Rows[0]["PickupCityShortName"].ToString();
                    _intDropOffCityID = Convert.ToInt32(dsBkgs.Tables[0].Rows[0]["DropOffCityID"]);
                    _dropoffCityShortName = dsBkgs.Tables[0].Rows[0]["DropOffCityShortName"].ToString();
                    
                    if (_intPickupCityID != 0)
                    {
                        lnkAssignedPickups.Visible = true;
                        lnkAssignedPickups.Text = ("Pickup: " + _pickupCityShortName + ", DropOff: " + _dropoffCityShortName).ToString();
                    }
                }

                foreach (DataRow drw in dsBkgs.Tables[1].Rows)
                {
                    string strVolume = drw["Volume"].ToString();

                    int intIsVolume = 0;
                    int intL = 0;
                    int intB = 0;
                    int intH = 0;

                    if (strVolume != "")
                    {
                        try
                        {
                            //l=30,b=30,h=30
                            string[] strvolumetric = strVolume.Split(',');

                            if (strvolumetric.Length == 3)
                            {
                                intL = Convert.ToInt32(strvolumetric[0].Split('=')[1]);
                                intB = Convert.ToInt32(strvolumetric[1].Split('=')[1]);
                                intH = Convert.ToInt32(strvolumetric[2].Split('=')[1]);
                                intIsVolume = 1;
                            }
                        }
                        catch (Exception)
                        {
                            intL = 0;
                            intB = 0;
                            intH = 0;
                            intIsVolume = 0;
                        }
                    }


                    AddNewRow(drw["ConsignmentTypeID"].ToString() + "-" + drw["Rate"].ToString(),
                                drw["MOPID"].ToString(),
                                drw["Description"].ToString(), drw["Qty"].ToString(),
                                drw["GoodsValue"].ToString(), drw["Rate"].ToString(), drw["Freight"].ToString(),
                                Convert.ToDouble(drw["ActualWeight"]), Convert.ToDouble(drw["ChargedWeight"]), intIsVolume, intL, intB, intH);
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
                    radDPayType.Enabled = false;
                    chkIsPartySender.Enabled = false;

                    txtNameSender.ReadOnly = true;
                    txtMobileNo.ReadOnly = true;

                    txtNameReceiver.ReadOnly = true;
                    txtMobileNoReceiver.ReadOnly = true;


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
                    txtEwayBillNo.ReadOnly = true;
                    radbtnEwayBillDate.Enabled = false;
                    //radDDDDBill.Enabled = false;
                    //radDDMMBill.Enabled = false;
                    //radDDYYBill.Enabled = false;
                }
                else if (intAllowUpdate == 1 && intAllowUpdateAll == 1 && ShowForUpdate)
                {
                    chkIsPartySender.Enabled = false;
                    chkIsPartyReceiver.Enabled = false;
                    radGridConsignItems.Enabled = false;

                    radDDBookingCity.Enabled = true;
                    radDDBookingBranch.Enabled = true;

                    if (blnIsOfflineMode)
                    {
                        if (intCompanyID == 805 || intCompanyID == 2921)
                        {
                            radDDDestCity.Enabled = false;
                            radDDDestBranch.Enabled = false;
                        }
                    }

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
                        //txtAddressReceiver.Enabled = true;
                    }
                    radDPayType.Enabled = true;


                    txtDeliveryChg.Enabled = true;
                    txtCartage.Enabled = true;
                    txtDocumentChg.Enabled = true;
                    txtInsurance.Enabled = true;
                    txtServiceTax.Enabled = false;
                    txtComment.Enabled = true;
                    radDModeofPayment.Enabled = false;

                    radBSave.Enabled = false;
                    //radBSave.Text = "Update";

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


                    txtDeliveryChg.ReadOnly = true;
                    txtCartage.ReadOnly = true;
                    txtDocumentChg.ReadOnly = true;
                    txtInsurance.ReadOnly = true;
                    txtServiceTax.ReadOnly = true;
                    txtComment.ReadOnly = true;
                    txtCartageDel.ReadOnly = true;

                    chkIsPartyReceiver.Enabled = false;
                    radDModeofPayment.Enabled = false;
                    radGridConsignItems.Enabled = false;
                    //radBSave.Enabled = false;

                    //radBSave.Text = "Update";

                    txtBillNo.ReadOnly = true;
                    dtBillNo.Enabled = false;
                    txtEwayBillNo.ReadOnly = true;
                    radbtnEwayBillDate.Enabled = false;
                    //radDDDDBill.Enabled = false;
                    //radDDMMBill.Enabled = false;
                    //radDDYYBill.Enabled = false;

                    txtSenderGSTN.Enabled = false;
                    txtReceiverGSTN.Enabled = false;
                    txtCollCartageRemark.Enabled = false;
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
            if (!blnIsOfflineMode)
                tableLayoutPanel1.BackColor = Color.Silver;
            //frmAdmin objFrmAdmin = (frmAdmin)Application.OpenForms["frmAdmin"];
            Cargo.frmBranchTransfer frm = new Cargo.frmBranchTransfer();
            //frm.Width = this.Width - 200;
            //frm.Height = this.Height - 200;
            frm.ShowDialog();
            this.Cursor = Cursors.Default;
            //objFrmAdmin.SetForm(frm);
            if (!blnIsOfflineMode)
                tableLayoutPanel1.BackColor = Color.White;
            tableLayoutPanel1.Enabled = true;
        }

        private void radBBranchReceipt_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            tableLayoutPanel1.Enabled = false;
            if (!blnIsOfflineMode)
                tableLayoutPanel1.BackColor = Color.Silver;
            //frmAdmin objFrmAdmin = (frmAdmin)Application.OpenForms["frmAdmin"];
            Cargo.frmBranchReceipt frm = new Cargo.frmBranchReceipt();
            //frm.Width = this.Width - 200;
            //frm.Height = this.Height - 200;
            frm.ShowDialog();
            this.Cursor = Cursors.Default;
            //objFrmAdmin.SetForm(frm);
            if (!blnIsOfflineMode)
                tableLayoutPanel1.BackColor = Color.White;
            tableLayoutPanel1.Enabled = true;
        }

        public void PrepareLinkLables(DataTable dt, int dtNo, int mode)
        {
            int intLastWidth = 0;
            LinkLabel lbl = null;
            Label lbl1 = null;

            foreach (DataRow dr in dt.Rows)
            {
                string strLblName = "";

                if (dtNo == 0 || dtNo == 2)
                {
                    if (dr["VehicleNo"].ToString() == "")
                        continue;
                    strLblName = "lnkLbl" + dr["LRNos"];
                }
                else
                {
                    if (dr["LRNo"].ToString() == "")
                        continue;
                    strLblName = "lnkLbl" + dr["LRNo"];
                }


                if (pnlMarqee.Controls.Find(strLblName, false).Length == 0)
                {
                    if (dtNo != 1)
                    {
                        lbl = new LinkLabel();
                        lbl.AutoSize = true;

                        lbl.LinkClicked += new LinkLabelLinkClickedEventHandler(linkLabel1_LinkClicked);
                        lbl.MouseHover += new EventHandler(linkLabel1_MouseHover);
                        lbl.MouseLeave += new EventHandler(linkLabel1_MouseLeave);

                        lbl.AutoSize = true;
                        lbl.Location = new System.Drawing.Point(intLastWidth, 4);
                        lbl.TabIndex = 0;
                        lbl.TabStop = true;
                        lbl.LinkColor = Color.Red;
                    }
                    else
                    {
                        lbl1 = new Label();
                        lbl1.AutoSize = true;
                        lbl1.Location = new System.Drawing.Point(intLastWidth, 4);
                        lbl1.TabIndex = 0;
                        lbl1.TabStop = true;
                        lbl1.ForeColor = Color.Red;
                    }

                    string type = "";
                    if (dtNo == 0 || dtNo == 2)
                        lbl.Name = "lnkLbl" + dr["BranchTransferID"].ToString();
                    else if (dtNo == 1)
                        lbl1.Name = "lnkLbl" + dr["LRNo"].ToString();

                    if (dr["Type"].ToString() == "SR" || dr["Type"].ToString() == "SR-B")
                        type = "Short Received By Branch ";
                    else if (dr["Type"].ToString() == "T")
                        type = "Branch Transfer By Branch ";
                    else if (dr["Type"].ToString() == "BR")
                        type = "At Branch Receipt : ";
                    else if (dr["Type"].ToString() == "R")
                        type = "At Receipt : ";

                    if (dtNo == 0)
                    {
                        if (mode == 2 || mode == 3)
                        {
                            lbl.Text = type + dr["ToBranch"].ToString() + " on " + dr["UpdateDate"].ToString() + " in Vehicle " + dr["VehicleNo"] + ". LRNos:" + dr["LRNos"].ToString();
                            lbl.Tag = dr["ToBranchID"] + "||" + dr["ToCityID"] + "||" + dr["UpdateDate"].ToString() + "||" + dr["Type"].ToString() + "||" + dr["DispatchIDs"].ToString();
                        }
                        else if (mode == 1)
                        {
                            lbl.Text = type + dr["FromBranch"].ToString() + " on " + dr["TransferDate"].ToString() + " in Vehicle " + dr["VehicleNo"] + ". LRNos:" + dr["LRNos"].ToString();
                            lbl.Tag = dr["FromBranchID"] + "||" + dr["VehicleNo"].ToString() + "||" + dr["TransferDate"].ToString() + "||" + dr["Type"].ToString();
                        }
                    }
                    else if (dtNo == 1)
                    {
                        lbl1.Text = type + "LR No   " + dr["LRNo"].ToString() + " " + dr["ReceiptDateTime"].ToString() +
                            ",   " + dr["ShortQty"] + " found short of " + dr["Items"].ToString() + ".";
                    }
                    else if (dtNo == 2)
                    {
                        lbl.Text = type + dr["FromBranch"].ToString() + " on " + dr["TransferDate"].ToString() + " in Vehicle " + dr["VehicleNo"] + ". LRNos:" + dr["LRNos"].ToString();
                        lbl.Tag = dr["FromBranchID"] + "||" + dr["VehicleNo"].ToString() + "||" + dr["TransferDate"].ToString() + "||" + dr["Type"].ToString();
                    }

                    if (dtNo != 1)
                    {
                        lbl.Font = new System.Drawing.Font("Verdana", 8);
                        SizeF size = lbl.CreateGraphics().MeasureString(lbl.Text, lbl.Font);
                        int newWidth = Convert.ToInt32(size.Width);
                        int newHeight = Convert.ToInt32(size.Height);
                        lbl.Size = new System.Drawing.Size(newWidth, newHeight);
                        lbl.Refresh();
                        intLastWidth = intLastWidth + newWidth + 30;
                        this.pnlMarqee.Controls.Add(lbl);
                        pnlMarqee.Refresh();
                    }
                    else
                    {
                        lbl1.Font = new System.Drawing.Font("Verdana", 8);
                        SizeF size = lbl1.CreateGraphics().MeasureString(lbl1.Text, lbl1.Font);
                        int newWidth = Convert.ToInt32(size.Width);
                        int newHeight = Convert.ToInt32(size.Height);
                        lbl1.Size = new System.Drawing.Size(newWidth, newHeight);
                        lbl1.Refresh();
                        intLastWidth = intLastWidth + newWidth + 30;
                        this.pnlMarqee.Controls.Add(lbl1);
                        pnlMarqee.Refresh();
                    }
                }
            }
        }

        public void GetAlerts(int mode)
        {
            try
            {
                DataSet ds = new DataSet();
                timer1.Stop();
                timer1.Enabled = false;


                for (int i = pnlMarqee.Controls.Count - 1; i >= 0; i--)
                {
                    if (pnlMarqee.Controls[i].Location.Y == 4)
                        pnlMarqee.Controls.RemoveAt(i);
                }

                if (mode == 1) //BranchTransfer
                    ds = App_Code.Cargo.GetBranchTransferedLuggageToReceive(0, intBranchID, intCompanyID, "".Trim(), 1, 1, "", intUserID, "", 0);
                else if (mode == 2) //ShortReceived
                    ds = App_Code.Cargo.GetBranchTransferedLuggageToReceive(0, intBranchID, intCompanyID, "".Trim(), 1, 100, "", intUserID, "", 0);
                else if (mode == 3) //Both (BranchTransfer and Short Received)
                    ds = App_Code.Cargo.GetBranchTransferedLuggageToReceive(0, intBranchID, intCompanyID, "".Trim(), 1, 3, "", intUserID, "", 0);

                if (mode == 2) //Short Received
                {
                    if (ds != null && ds.Tables.Count > 1)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                            PrepareLinkLables(ds.Tables[0], 0, mode);
                        // if (ds.Tables[1].Rows.Count > 0)
                        //     PrepareLinkLables(ds.Tables[1], 1, mode);
                    }
                }
                else if (mode == 1) //Branch Transfer
                {
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                            PrepareLinkLables(ds.Tables[0], 0, mode);
                    }
                }
                else if (mode == 3) //Both
                {
                    if (ds != null && ds.Tables.Count > 2)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                            PrepareLinkLables(ds.Tables[0], 0, mode);
                        //   if (ds.Tables[1].Rows.Count > 0)
                        //       PrepareLinkLables(ds.Tables[1], 1, mode);
                        if (ds.Tables[2].Rows.Count > 0)
                            PrepareLinkLables(ds.Tables[2], 2, mode);
                    }
                }
                timer1.Enabled = true;
                timer1.Start();
                timerGetTransfered.Enabled = false;
                timerGetTransfered.Stop();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
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
                            //lbl.LinkColor = Color.Azure;

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

                    ds = App_Code.Cargo.GetBranchTransferedLuggageToReceive(0, intBranchID, intCompanyID, "".Trim(), 1, 100, "", intUserID, "", 0);
                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
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
                                lbl.LinkColor = Color.Red;

                                string typeOfDispatch = "Branch Transfer By :";

                                if (dr["Type"].ToString() == "D")
                                    typeOfDispatch = "Dispatch By :";

                                lbl.Text = typeOfDispatch + dr["ToBranch"].ToString() + " " + dr["UpdateDate"].ToString() + " in Vehicle " + dr["VehicleNo"] + ". LRNos:" + dr["LRNos"].ToString();
                                lbl.Tag = dr["ToBranchID"] + "||" + dr["VehicleNo"].ToString() + "||" + dr["UpdateDate"].ToString() + "||" + dr["Type"].ToString();
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
                if (HasShortReceiptMarquee == 1 && HasBranchTransferMarquee == 1)
                {
                    GetAlerts(3);
                }
                else if (HasBranchTransferMarquee == 1)
                {
                    GetAlerts(1);
                }
                else if (HasShortReceiptMarquee == 1)
                {
                    GetAlerts(2);
                }
                //GetTransferAlerts();
                pnlMarqee.Location = new System.Drawing.Point(xPos, 50);
            }
            else
            {

                pnlMarqee.Location = new System.Drawing.Point(xPos, 50);
                xPos = xPos - 1;
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

            int intToBranchID, intToCityID, intFromBranchID;
            intToBranchID = intToCityID = intFromBranchID = 0;

            string strDispatchIDs = ""; string strVehicle = "";

            string[] tagVal = ((LinkLabel)sender).Tag.ToString().Replace("||", "~").Split('~');
            string strType = Convert.ToString(tagVal[3]);
            string strTransDate = Convert.ToString(tagVal[2]);

            if (strType == "SR" || strType == "SR-B")
            {
                intToBranchID = Convert.ToInt32(tagVal[0]);
                intToCityID = Convert.ToInt32(tagVal[1]);
                strDispatchIDs = tagVal[4];
            }
            else if (strType == "T")
            {
                intFromBranchID = Convert.ToInt32(tagVal[0]);
                strVehicle = Convert.ToString(tagVal[1]);
            }

            //dr["FromBranchID"] + "||" + dr["VehicleNo"].ToString() + "||" + dr["TransferDate"].ToString();

            this.Cursor = Cursors.WaitCursor;
            tableLayoutPanel1.Enabled = false;
            if (!blnIsOfflineMode)
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
            else if (strType == "SR" || strType == "SR-B")
            {
                Cargo.frmDispatch frm = new Cargo.frmDispatch(strDispatchIDs, intToBranchID, intToCityID, strTransDate);
                frm.ShowDialog();
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
            if (!blnIsOfflineMode)
                tableLayoutPanel1.BackColor = Color.White;
            tableLayoutPanel1.Enabled = true;


            timer1.Start();
            ((LinkLabel)sender).LinkColor = Color.Green;
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
                xPos1 = panel1.Width;
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


                                this.pnlMarqeeShortReceipt.Controls.Add(lbl);

                                //pnlMarqeeShortReceipt.Size = new System.Drawing.Size(intLastWidth, pnlMarqee.Height);
                                pnlMarqeeShortReceipt.Refresh();
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

                    timer2.Enabled = true;
                    timer2.Start();
                    timerGetShortReceived.Enabled = false;
                    timerGetShortReceived.Stop();
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
                //lblCollChgBx.Visible = true;
                //txtCollectionChg.Visible = true;
                radDCollectionType.Focus();
            }
            else
            {
                //lblCollChgBx.Visible = false;
                //txtCollectionChg.Visible = false;
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

                    if ((EnteredRecMobNumber == 0000000000) || ((EnteredRecMobNumber / (EnteredRecMobNumber % 10)) == 1111111111)) //|| (txtMobileNoReceiver.Text[0] - '0') < 7
                    {
                        MessageBox.Show("Invalid Receiver Mobile Number. Please re-enter.");
                        txtMobileNoReceiver.Focus();
                        return;
                    }
                    else
                    {
                        if (txtMobileNoReceiver.Text.ToString().Length == 10 && chkRecMobileGetData.Checked == true)
                        {
                            GetCustomerDetails("R");
                        }
                    }
                }
            }
            catch (Exception ex)
            { }
        }

        private void radDCollectionType_SelectedIndexChanged(object sender, Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
        {
            //int SelectedIndex = ((RadDropDownList)sender).SelectedIndex;

            //if (SelectedIndex == 2)
            //{
            //    txtCollectionChg.Enabled = false;
            //    txtCollectionChg.Text = "0";
            //}
            //else
            //{
            //    txtCollectionChg.Enabled = true;
            //    txtCollectionChg.Text = "0";
            //}
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
                        //MessageBox.Show("Please enter Receiver Mobile No.");
                        //chkBx.Checked = false;
                        //txtMobileNoReceiver.Focus();
                    }
                    //else if (txtMobileNoReceiver.Text.Length != 10)
                    //{
                    //    MessageBox.Show("Mobile No. should have 10 digit.");
                    //    chkBx.Checked = false;
                    //    txtMobileNoReceiver.Focus();
                    //}
                    else
                    {
                        GetCustomerDetails("R");
                        //this.Cursor = Cursors.WaitCursor;
                        //DataSet ds = new DataSet();

                        //ds = App_Code.Cargo.GetNameAndAddress("R", txtMobileNoReceiver.Text, "", intCompanyID, intBranchID, intUserID);

                        //if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        //{
                        //    if (txtAddressReceiver.Visible)
                        //        txtAddressReceiver.Text = ds.Tables[0].Rows[0]["RecAddress"].ToString();

                        //    if (is_stax_company && is_GSTN_branch == 1 && ds.Tables[0].Rows[0]["ReceiverGSTN"].ToString().Trim() != "")
                        //    {
                        //        txtReceiverGSTN.Text = ds.Tables[0].Rows[0]["ReceiverGSTN"].ToString();
                        //        SetGSTPaidBy();
                        //    }

                        //    txtNameReceiver.Text = ds.Tables[0].Rows[0]["RecName"].ToString();
                        //}
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

        void LoadOfflineBooking(bool flag)
        {
            radDDBookingCity.Enabled = flag;
            radDDBookingBranch.Enabled = flag;

            if (flag)
                tableLayoutPanel1.BackColor = objColorOffline;
            else
            {
                tableLayoutPanel1.BackColor = objColorWhite;
                radBOfflineBooking.Text = "Offline Booking";
            }

            //grpBxSenderInfo.BackColor = objColorOffline;
            //grpBxReceiverInfo.BackColor = objColorOffline;
            //grpBxConsignmentItems.BackColor = objColorOffline;
            //grpBXPayment.BackColor = objColorOffline;
            //grpCharges.BackColor = objColorOffline;



            lblManualLR.Visible = flag;
            txtManualLR.Visible = flag;

            //if (flag)
            //{
            //    tblLPManualLR.ColumnStyles[0].Width = 122;
            //    tblLPManualLR.ColumnStyles[1].Width = 45;
            //}
            //else
            //{
            //    tblLPManualLR.ColumnStyles[0].Width = 300;
            //    tblLPManualLR.ColumnStyles[1].Width = 0;
            //}

            ShowHideOfflineBookingUser(flag);

            lblVehicle.Visible = flag;
            tlpVehicle.Visible = flag;
            lblIsParty.Visible = !flag;
            chkIsPartySender.Visible = !flag;
            lblIsParty2.Visible = !flag;
            chkIsPartyReceiver.Visible = !flag;
            label4.Visible = !flag;
            txtSenderEmailID.Visible = !flag;
            label5.Visible = !flag;
            txtReceiverEmailID.Visible = !flag;
            lblBillNo.Visible = !flag;
            txtBillNo.Visible = !flag;
            lblStaxPaidBy.Visible = !flag;
            radDDCStaxPaidBy.Visible = !flag;

            if (is_stax_company && is_GSTN_branch == 1 && radDDCStaxPaidBy.Visible)
                radDDCStaxPaidBy.SelectedValue = 2;
            else
                radDDCStaxPaidBy.SelectedValue = 1;

            lblCartageBx.Visible = !flag;
            txtCartage.Visible = !flag;
            label2.Visible = !flag;
            txtCollCartageRemark.Visible = !flag;
            lblCartageDel.Visible = !flag;
            txtCartageDel.Visible = !flag;
            lblInsuranceBx.Visible = !flag;
            txtInsurance.Visible = !flag;
            lblDocChgBx.Visible = !flag;
            txtDocumentChg.Visible = !flag;
            lblServiceTxBx.Visible = !flag;
            txtServiceTax.Visible = !flag;
            lblLastLR.Visible = !flag;
            lblLastLRNo.Visible = !flag;

        }

        private void radBOfflineBooking_Click(object sender, EventArgs e)
        {

            this.Cursor = Cursors.WaitCursor;

            if (blnIsSTACargoCompany)
            {
                if (radBOfflineBooking.Text == "Online Booking")
                {
                    blnIsOfflineMode = false;

                    FillBookingBranch(false);

                    FillDestCities(false);
                    FillBookingCity(false);

                    LoadOfflineBooking(false);

                    FillBookingCity();
                    radDDBookingCity.SelectedValue = intBranchCityID.ToString();

                    FillBookingBranch();
                    radDDBookingBranch.SelectedValue = intBranchID.ToString();

                    radBOfflineBooking.Text = "Offline Booking";

                    RemoveValuesFromControls(false);

                    radDDDestCity.Enabled = true;
                    radDDDestBranch.Enabled = true;
                }
                else
                {
                    blnIsOfflineMode = true;

                    FillBookingBranch(true);
                    FillDestCities(true);
                    FillBookingCity(true);

                    LoadOfflineBooking(true);

                    radBOfflineBooking.Text = "Online Booking";

                    RemoveValuesFromControls(true);

                    lblLRNo.Visible = true;
                    lblLRNo.Text = "Offline Booking";
                }

            }
            else
            {
                tableLayoutPanel1.Enabled = false;
                if (!blnIsOfflineMode)
                    tableLayoutPanel1.BackColor = Color.Silver;
                //frmAdmin objFrmAdmin = (frmAdmin)Application.OpenForms["frmAdmin"];
                Cargo.frmLuggageBookingManual frm = new Cargo.frmLuggageBookingManual();
                //frm.Width = this.Width - 200;
                //frm.Height = this.Height - 200;
                frm.ShowDialog();


                //objFrmAdmin.SetForm(frm);
                if (!blnIsOfflineMode)
                    tableLayoutPanel1.BackColor = Color.White;
                tableLayoutPanel1.Enabled = true;
            }
            this.Cursor = Cursors.Default;
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

        //private void radDDCrossingCity_SelectedIndexChanged(object sender, Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
        //{
        //    try
        //    {
        //        if (radDDCrossingCity.SelectedIndex == -1)
        //            return;

        //        this.Cursor = Cursors.WaitCursor;
        //        //FillCrossingBranch();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //    finally
        //    {
        //        this.Cursor = Cursors.Default;
        //    }

        //}

        //private void chkCrossing_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (chkCrossing.Checked)
        //    {
        //        if (intCompanyID == 1 || is_crossing_company)
        //        {
        //            lblCrossingCity.Visible = true;
        //            lblCrossingBranch.Visible = true;
        //            radDDCrossingCity.Visible = true;
        //            radDDCrossingBranch.Visible = true;
        //        }
        //    }
        //    else
        //    {
        //        lblCrossingCity.Visible = false;
        //        lblCrossingBranch.Visible = false;
        //        radDDCrossingCity.Visible = false;
        //        radDDCrossingBranch.Visible = false;

        //    }
        //}

        private void btnChangeVehicle_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                tableLayoutPanel1.Enabled = false;
                //CRS2011DeskApp.Cargo.frmChangeVehicle frm = new CRS2011DeskApp.Cargo.frmChangeVehicle();
                ////objFrm.Size = new System.Drawing.Size(Convert.ToInt32(this.Width / 2), Convert.ToInt32(this.Height / 2));

                CRS2011DeskApp.Cargo.frmAdminUpdate frm = new CRS2011DeskApp.Cargo.frmAdminUpdate();

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



        private void radGridConsignItems_UserDeletedRow(object sender, GridViewRowEventArgs e)
        {
            setAdditionalCharges();
            SetDocumentCharge(Convert.ToInt32(radDPayType.SelectedValue));

        }

        private void radbtnRequestRecharge_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                tableLayoutPanel1.Enabled = false;
                CRS2011DeskApp.frmRechargeBranch frm = new CRS2011DeskApp.frmRechargeBranch();

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

        private void radbtnUpdateRecharge_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                tableLayoutPanel1.Enabled = false;
                CRS2011DeskApp.frmRechargeConfirmBranch frm = new CRS2011DeskApp.frmRechargeConfirmBranch();

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

        private void btnLoadChalan_Click(object sender, EventArgs e)
        {
            if (txtLRNo.Text == "")
            {
                MessageBox.Show("Please Enter Chalan No.");
                return;
            }
            this.Cursor = Cursors.WaitCursor;

            try
            {
                int intChalanNo = Convert.ToInt32(txtLRNo.Text);

                frmLoadChalanNo objFrm = new frmLoadChalanNo(intChalanNo);
                objFrm.StartPosition = FormStartPosition.CenterScreen;

                DialogResult dlg = objFrm.ShowDialog();

                if (dlg == System.Windows.Forms.DialogResult.Cancel)
                    return;
                else
                {
                    txtLRNo.Text = objFrm.LRNO;
                    radBShow_Click(null, null);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Invalid Chalan No.");
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void timerGetLastLR_Tick(object sender, EventArgs e)
        {
            try
            {
                dlgShowLastLRNO TG = new dlgShowLastLRNO(showLastLRNo);
                TG.BeginInvoke(null, null);
            }
            catch (Exception ex)
            {
            }
        }


        private void showLastLRNo()
        {
            try
            {
                string strLastLRNo = "";
                string strFormattedLRNo = "";
                DataSet ds = App_Code.Cargo.getLastLR(intCompanyID, intBranchID, intUserID);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    strLastLRNo = ds.Tables[0].Rows[0]["LRNO"].ToString();
                    strFormattedLRNo = ds.Tables[0].Rows[0]["FormattedLRNo"].ToString();

                    strLastLRNo = Common.FormattedLR(strLastLRNo);
                }
                else
                {
                    strLastLRNo = "";
                    strFormattedLRNo = "";
                }


                MethodInvoker mi = delegate
                        {
                            lblLastLRNo.Text = strFormattedLRNo;
                        };
                this.Invoke(mi);
            }
            catch (Exception)
            {

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

        private void radbtnblocklr_Click(object sender, EventArgs e)
        {
            if (chkIsPartySender.Checked && Convert.ToInt32(radDPartySender.SelectedValue.ToString().Split('^')[0]) > 0)
            {
                DialogResult dr = MessageBox.Show("Are you sure you want to Block LR for Party " + radDPartySender.SelectedItem.Text + "?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dr == DialogResult.Yes)
                {
                    string strLRNo = "";
                    int intBlockedID = 0;
                    int intPartyID = Convert.ToInt32(radDPartySender.SelectedValue.ToString().Split('^')[0]);

                    DataSet ds = App_Code.Cargo.CargoBlockLRNo(intCompanyID, intBranchID, intUserID, "", Common.GetCacheGUID(), intPartyID);
                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        strLRNo = ds.Tables[0].Rows[0]["LRNO"].ToString();
                        intBlockedID = Convert.ToInt32(ds.Tables[0].Rows[0]["BlockedID"].ToString());

                        string strMsg = "Blocked LRNo is : " + strLRNo + ", Please enter remarks : ";
                        string promptValue = Prompt.ShowDialog(strMsg, "Blocked LR");

                        if (promptValue != "")
                        {
                            App_Code.Cargo.CargoBlockLRNoRemark(intBlockedID, intUserID, promptValue);

                            MessageBox.Show("LR Remarks saved successfully");
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select Party for Block LR");
            }
        }

        private void txtCollCartageRemark_Leave(object sender, EventArgs e)
        {
            if (IsValidCharacters(this.txtCollCartageRemark))
            {
                if (txtCollCartageRemark.Text.Length == 0)
                {
                    txtCollCartageRemark.Text = "Cartage - Mobile and Comment";
                    txtCollCartageRemark.ForeColor = SystemColors.GrayText;
                }
            }
            else
            {
                MessageBox.Show("Special Characters('~','^','|','&') can not be used.", "Oops...");
                txtCollCartageRemark.Focus();
                return;
            }
        }

        private void txtCollCartageRemark_Enter(object sender, EventArgs e)
        {
            if (txtCollCartageRemark.Text == "Cartage - Mobile and Comment")
            {
                txtCollCartageRemark.Text = "";
                txtCollCartageRemark.ForeColor = SystemColors.WindowText;
            }
        }

        private void checkBox_Highlight(object sender, EventArgs e)
        {
            CheckBox control = (CheckBox)sender;
            control.FlatStyle = FlatStyle.Flat;
            control.ForeColor = Color.Blue;
        }

        private void checkBox_EndHighlight(object sender, EventArgs e)
        {
            CheckBox control = (CheckBox)sender;
            if (!control.Focused)
            {
                control.ForeColor = DefaultForeColor;
            }
        }

        private void radBtnCommonLR_Click(object sender, EventArgs e)
        {
            Cargo.frmCommonLRSearch frm = new Cargo.frmCommonLRSearch();
            frm.Width = Convert.ToInt32(this.Width * 0.95);
            frm.Height = Convert.ToInt32(this.Height * 0.90);
            frm.ShowDialog();
        }

        private void radDDBookingCity_SelectedIndexChanged(object sender, Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
        {
            try
            {
                if (radDDBookingCity.SelectedIndex == -1)
                    return;

                this.Cursor = Cursors.WaitCursor;
                FillBookingBranch(true);

                txtSenderAddress.Text = radDDBookingCity.Text;

                //FillCrossingCities();
                //FillPartiesforDestCity();
                //fillPayType();

                //txtAddressReceiver.Text = radDDDestCity.Text;

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

        private void txtManualLR_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (blnIsSTACargoCompany && blnIsOfflineMode)
                e.Handled = Common.AllowNumeric(e);
        }


        private void LoadVehicles()
        {

            dsVehicleNo = App_Code.Cargo.GetVehicleNo(intCompanyID, 0, 0, DateTime.Now.AddMinutes(0), 1);

            if (dsVehicleNo != null && dsVehicleNo.Tables.Count > 0 && dsVehicleNo.Tables[0].Rows.Count > 0)
            {
                DataRow dr = dsVehicleNo.Tables[0].NewRow();
                dr["BusNumber"] = "--Select--";
                dsVehicleNo.Tables[0].Rows.InsertAt(dr, 0);

                radDVehicleNos.DisplayMember = "BusNumber";
                radDVehicleNos.ValueMember = "VehicleID";
                radDVehicleNos.DataSource = dsVehicleNo.Tables[0];

            }
        }

        private void txtBusNoSearch1_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (dsVehicleNo != null && dsVehicleNo.Tables[0].Rows.Count > 0)
                {
                    DataView dv = dsVehicleNo.Tables[0].Copy().DefaultView;
                    radDVehicleNos.DataSource = dsVehicleNo.Tables[0];
                    radDVehicleNos.DisplayMember = "BusNumber";
                    radDVehicleNos.ValueMember = "VehicleID";
                    dv.RowFilter = "BusNumber LIKE '%" + txtBusNoSearch1.Text + "%'";
                    DataTable dtFilteredBusList = dv.ToTable();

                    if (dtFilteredBusList.Rows.Count > 0)
                    {
                        radDVehicleNos.DataSource = dtFilteredBusList;
                        radDVehicleNos.SelectedIndex = 0;
                    }
                    else
                    {
                        radDVehicleNos.DataSource = null;
                    }
                    radDVehicleNos.Focus();
                }

                txtBusNoSearch1.Focus();

            }
            catch (Exception ex)
            {
                radDVehicleNos.DataSource = null;
                txtBusNoSearch1.Focus();
            }
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            if (blnIsFirstLoad)
            {
                lblOperatorSetMarquee.Location = new System.Drawing.Point(pnlDefaultMarquee.Size.Width, lblOperatorSetMarquee.Location.Y);
                xPos2 = pnlDefaultMarquee.Size.Width;
                blnIsFirstLoad = false;
            }
            else
            {
                if (xPos2 + lblOperatorSetMarquee.Width <= 0)
                {
                    //repeat marquee
                    lblOperatorSetMarquee.Location = new System.Drawing.Point(pnlDefaultMarquee.Size.Width, lblOperatorSetMarquee.Location.Y);
                    xPos2 = pnlDefaultMarquee.Size.Width;
                }
                else
                {
                    lblOperatorSetMarquee.Location = new System.Drawing.Point(xPos2, lblOperatorSetMarquee.Location.Y);
                    xPos2 -= 1;
                }
            }
        }

        private void txtHamaliChg_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = Common.AllowNumeric(e);
        }

        private void radDDBookingBranch_SelectedIndexChanged(object sender, Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
        {
            try
            {
                if (radDDBookingBranch.Items.Count > 0)
                {
                    int BranchID = Convert.ToInt32(radDDBookingBranch.SelectedValue);
                    int CompanyID = 0;
                    DataTable dt = App_Code.Cargo.GetBranchDetails(BranchID);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        CompanyID = Convert.ToInt32(dt.Rows[0]["companyid"].ToString());
                        if (CompanyID == intCompanyID)
                            CompanyID = 0;
                    }

                    DataTable dtTempUsers = Common.GetBranchUsers(radDDBookingBranch.SelectedValue.ToString(), "B", null, CompanyID);
                    DataTable dtUsrs1 = new DataView(dtTempUsers, "", "UserName ASC", DataViewRowState.CurrentRows).ToTable(true, "UserID", "UserName");

                    radDDOfflineBookingUser.DataSource = null;

                    if (dtUsrs1.Rows.Count > 0)
                    {
                        radDDOfflineBookingUser.DisplayMember = "UserName";
                        radDDOfflineBookingUser.ValueMember = "UserID";
                        radDDOfflineBookingUser.DataSource = dtUsrs1;
                    }
                }
                else
                    radDDOfflineBookingUser.DataSource = null;
            }
            catch (Exception)
            {

            }
        }

        private void txtSenderGSTN_Leave(object sender, EventArgs e)
        {
            string sender_gstn = txtSenderGSTN.Text.ToUpper();
            int gstn_len = sender_gstn.Length;

            if (gstn_len != 0 && gstn_len != 15)
            {
                MessageBox.Show("Invalid GSTN. GSTN should be in '22AAAAA0000A1Z5' format.");
                txtSenderGSTN.Focus();
                return;
            }
            else if (gstn_len == 0)
            {
                if (lblStaxPaidBy.Visible && radDDCStaxPaidBy.Visible && is_stax_company && is_GSTN_branch == 1)
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
                    if (lblStaxPaidBy.Visible && radDDCStaxPaidBy.Visible && is_stax_company && is_GSTN_branch == 1)
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

            if (gstn_len != 0 && gstn_len != 15)
            {
                MessageBox.Show("Invalid GSTN. GSTN should be in '22AAAAA0000A1Z5' format.");
                txtReceiverGSTN.Focus();
                return;
            }
            else if (gstn_len == 0)
            {
                if (lblStaxPaidBy.Visible && radDDCStaxPaidBy.Visible && is_stax_company && is_GSTN_branch == 1)
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
                    if (lblStaxPaidBy.Visible && radDDCStaxPaidBy.Visible && is_stax_company && is_GSTN_branch == 1)
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

        private void SetGSTPaidBy()
        {
            try
            {
                txtServiceTax.Text = "0";

                if (lblStaxPaidBy.Visible && radDDCStaxPaidBy.Visible && is_stax_company && is_GSTN_branch == 1)
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

                    if ((EnteredSenMobNumber == 0000000000) || ((EnteredSenMobNumber / (EnteredSenMobNumber % 10)) == 1111111111)) //|| (txtMobileNo.Text[0] - '0') < 7
                    {
                        MessageBox.Show("Invalid Sender Mobile Number. Please re-enter.");
                        txtMobileNo.Focus();
                        return;
                    }
                    else if (txtMobileNo.Text.ToString().Length == 10 && chkSenderMobileGetData.Checked == true)
                    {
                        GetCustomerDetails("S");
                    }
                }
            }
            catch (Exception ex)
            { }
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

                            if (is_stax_company && is_GSTN_branch == 1)
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

                            if (is_stax_company && is_GSTN_branch == 1)
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

        private void txtFreightChg_TextChanged(object sender, EventArgs e)
        {
            if (intCompanyID == 66 || intCompanyID == 184 || intCompanyID == 805 || intCompanyID == 2649 || intCompanyID == 2650 || intCompanyID == 322)
                SetDocumentCharge(Convert.ToInt32(radDPayType.SelectedValue));
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

        private Boolean IsValidCharacters(Control txtbox)
        {
            if (SpecialChars.Any(item => (txtbox.Text.ToString()).Contains(item)))
                return false;
            else
                return true;
        }

        private void txtComment_Leave(object sender, EventArgs e)
        {
            if (!IsValidCharacters(this.txtComment))
            {
                MessageBox.Show("Special Characters('~','^','|','&') can not be used.", "Oops...");
                txtComment.Focus();
                return;
            }
        }

        private void radGridConsignItems_CommandCellClick(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                Telerik.WinControls.UI.GridViewCellEventArgs ele = (Telerik.WinControls.UI.GridViewCellEventArgs)e;
                if (ele.Column.Name.ToString().ToUpper() == "WEIGHTVOLUME")
                {
                    if (radGridConsignItems.Rows[ele.RowIndex].Cells["Length"].Value == null || radGridConsignItems.Rows[ele.RowIndex].Cells["Length"].Value == "")
                        radGridConsignItems.Rows[ele.RowIndex].Cells["Length"].Value = "0";
                    if (radGridConsignItems.Rows[ele.RowIndex].Cells["Width"].Value == null || radGridConsignItems.Rows[ele.RowIndex].Cells["Width"].Value == "")
                        radGridConsignItems.Rows[ele.RowIndex].Cells["Width"].Value = "0";
                    if (radGridConsignItems.Rows[ele.RowIndex].Cells["Height"].Value == null || radGridConsignItems.Rows[ele.RowIndex].Cells["Height"].Value == "")
                        radGridConsignItems.Rows[ele.RowIndex].Cells["Height"].Value = "0";

                    Cargo.frmVolumetricData frm = new Cargo.frmVolumetricData(Convert.ToDecimal(radGridConsignItems.Rows[ele.RowIndex].Cells["Length"].Value),
                                                                             Convert.ToDecimal(radGridConsignItems.Rows[ele.RowIndex].Cells["Width"].Value),
                                                                             Convert.ToDecimal(radGridConsignItems.Rows[ele.RowIndex].Cells["Height"].Value), Convert.ToDecimal(radGridConsignItems.Rows[ele.RowIndex].Cells["Weight"].Value), this);
                    frm.ShowDialog();

                    radGridConsignItems.Rows[ele.RowIndex].Cells["Weight"].Value = dcmVolumetricWeight.ToString();
                    radGridConsignItems.Rows[ele.RowIndex].Cells["Length"].Value = dcmLength.ToString();
                    radGridConsignItems.Rows[ele.RowIndex].Cells["Width"].Value = dcmWidth.ToString();
                    radGridConsignItems.Rows[ele.RowIndex].Cells["Height"].Value = dcmHeight.ToString();


                    if (dcmVolumetricWeight > 0)
                    {
                        if (blnIsSTACargoCompany)
                        {
                            // SetDocumentCharge(Convert.ToInt32(radDPayType.SelectedValue));

                            radGridConsignItems.Columns["WeightChrg"].ReadOnly = true;  //6

                            if (radGridConsignItems.Rows[ele.RowIndex].Cells["Weight"].Value.ToString() == "")
                                radGridConsignItems.Rows[ele.RowIndex].Cells["Weight"].Value = 0;

                            int intQty = Convert.ToInt32(radGridConsignItems.Rows[ele.RowIndex].Cells["Qty"].Value); // Convert.ToInt32(e.Row.Cells["Qty"].Value.ToString());

                            int intActWeight = Convert.ToInt32(radGridConsignItems.Rows[ele.RowIndex].Cells["Weight"].Value);

                            int intMinAllUnitChargeWeight = Convert.ToInt32(radGridConsignItems.Rows[ele.RowIndex].Cells["MinWeightAllUnits"].Value);
                            int intMinPerUnitChargeWeight = Convert.ToInt32(radGridConsignItems.Rows[ele.RowIndex].Cells["MinWeightPerUnit"].Value);

                            int intCalChrgWeight = 0;

                            if (intMinPerUnitChargeWeight > 0)
                                intCalChrgWeight = intQty * intMinPerUnitChargeWeight;
                            else
                                intCalChrgWeight = intMinAllUnitChargeWeight;


                            if (intCalChrgWeight < intActWeight)
                            {
                                radGridConsignItems.Rows[ele.RowIndex].Cells["WeightChrg"].Value = intActWeight;
                            }
                            else
                            {
                                radGridConsignItems.Rows[ele.RowIndex].Cells["WeightChrg"].Value = intCalChrgWeight;
                            }
                        }

                        if (radGridConsignItems.Columns["WeightChrg"].IsVisible && !radGridConsignItems.Columns["WeightChrg"].ReadOnly)
                            radGridConsignItems.Rows[ele.RowIndex].Cells["WeightChrg"].BeginEdit();
                        else
                            radGridConsignItems.Rows[ele.RowIndex].Cells["Rate"].BeginEdit();
                    }
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

        private void radGridConsignItems_CellClick(object sender, GridViewCellEventArgs e)
        {

        }

        private void radGridConsignItems_CellFormatting(object sender, CellFormattingEventArgs e)
        {
            e.CellElement.Enabled = true;
            if (e.RowIndex > -1 && e.Column.Name == "WeightVolume")
            {
                if (Convert.ToInt32(e.Row.Cells["IsVolumetricWeight"].Value) == 1)
                {
                    e.CellElement.Enabled = true;
                }
                else
                {
                    e.CellElement.Enabled = false;
                }
            }

            //if (e.RowIndex > -1 && e.Column.Name == "EditDimensions")
            //{
            //    if (Convert.ToInt32(e.Row.Cells["IsVolumetricWeight"].Value) == 1)
            //    {
            //        e.CellElement.Enabled = true;
            //    }
            //    else
            //    {
            //        e.CellElement.Enabled = false;
            //    }
            //}

        }

        public void SetVolumetricData(decimal _dcmLength, decimal _dcmWidth, decimal _dcmHeight, decimal _dcmVolumetricWeight)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                dcmLength = _dcmLength;
                dcmWidth = _dcmWidth;
                dcmHeight = _dcmHeight;
                dcmVolumetricWeight = _dcmVolumetricWeight;

            }
            catch (Exception ex)
            {

            }
            finally
            {
                this.Cursor = Cursors.Default;
            }

        }


        public void ReprintSticker(DataTable dtBkgdet, int BookingID, string LRNO)
        {
            try
            {
                DialogResult dr2 = MessageBox.Show("Do you want to take a Sticker print?", "Sticker Print", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr2 == DialogResult.Yes)
                {
                    int totalunit = Convert.ToInt32(dtBkgdet.Rows[0]["NoA"].ToString());
                    string strMsg = "How many Sticker want to print?";
                    string promptValue = Prompt.ShowDialog(strMsg, "Sticker Print", "Print");
                    try
                    {
                        int cnt = Convert.ToInt32(promptValue);

                        if (cnt > 0 && cnt <= totalunit)
                        {
                            Cargo.frmPrintTicket frmPrint = new Cargo.frmPrintTicket(BookingID, intBranchID, intUserID,
                                           LRNO, dtBkgdet, true, strLaserPrinterName, strStickerPrinterName, "", cnt);
                            frmPrint.ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show("You can not print more then total units " + totalunit + "!", "Invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception ex)
            { }
        }

        private void chkFTLAssignCities_CheckedChanged(object sender, EventArgs e)
        {
            if (chkFTLAssignCities.Checked)
            {
                string Apc = radDDBookingCity.SelectedValue.ToString() + '~';
                string Adc = radDDDestCity.SelectedValue.ToString() + '~';

                Cargo.frmFTLPickupAndDropoff frm = new Cargo.frmFTLPickupAndDropoff(this, Apc, Adc, radDPayType.SelectedValue.ToString(), false);
                frm.ShowDialog();
                if (lnkAssignedPickups.Text.Trim() == "")
                    chkFTLAssignCities.Checked = false;
            }
            else
            {
                _intPickupCityID = 0;
                _intDropOffCityID = 0;
                _pickupCityShortName = "";
                _dropoffCityShortName = "";
                lnkAssignedPickups.Text = "";
                lnkAssignedPickups.Visible = false;
            }
            ConfigureGridAsFTL();
        }

        public void _AssignPickupCityAndBranch(int pickupCityID, string pickupcityshortname, int dropoffCityID, string dropOffCityShortName)
        {
            if (pickupCityID == 0)
                chkFTLAssignCities.Checked = false;

            _intPickupCityID = pickupCityID;
            _pickupCityShortName = pickupcityshortname;
            _intDropOffCityID = dropoffCityID;
            _dropoffCityShortName = dropOffCityShortName;

            if (_intPickupCityID != 0)
            {
                lnkAssignedPickups.Visible = true;
                lnkAssignedPickups.Text = ("Pickup: " + _pickupCityShortName + ", DropOff: " + _dropoffCityShortName).ToString();
            }
        }

        private void lnkAssignedPickups_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (lnkAssignedPickups.Text.Trim() != "")
            {
                //if ((lnkAssignedPickups.Text.Split(',')[0]).Split(':')[0].Trim() == "Pickup")
                //{
                    string Apc = _intPickupCityID.ToString() + '~' + ((lnkAssignedPickups.Text.Split(',')[0]).Split(':')[1].Trim()).ToString();
                    string Adc = _intDropOffCityID.ToString() + '~' + ((lnkAssignedPickups.Text.Split(',')[1]).Split(':')[1].Trim()).ToString();
                    Cargo.frmFTLPickupAndDropoff frm = new Cargo.frmFTLPickupAndDropoff(this, Apc, Adc, radDPayType.SelectedValue.ToString(), true);
                    frm.ShowDialog();
                //}
                //else if ((lnkAssignedPickups.Text.Split(',')[0]).Split(':')[0].Trim() == "PickupID") //this block is for show lr 
                //{
                //    string Apc = _intPickupCityID.ToString() + '~';
                //    string Adc = _intDropOffCityID.ToString() + '~';
                //    Cargo.frmFTLPickupAndDropoff frm = new Cargo.frmFTLPickupAndDropoff(this, Apc, Adc, radDPayType.SelectedValue.ToString(), false);
                //    frm.ShowDialog();
                //}

                if (lnkAssignedPickups.Text.Trim() == "")
                    chkFTLAssignCities.Checked = false;

                ConfigureGridAsFTL();
            }
        }

        private void ConfigureGridAsFTL()
        {
            int intIsMOPEditable = 0, intDefaultMOPId = 0, intBillingUnit = 1, intMinAllUnitChargeWeight = 0, intMinPerUnitChargeWeight = 0, intIsVolumetricWeight = 0;

            try
            {
                radGridConsignItems.Rows[0].Cells["ConsignmentType"].Value = 0;
                radGridConsignItems.Rows[0].Cells["MOP"].Value = 0;
                radGridConsignItems.Rows[0].Cells["Qty"].Value = 0;
                radGridConsignItems.Rows[0].Cells["GoodsValue"].Value = 0;
                radGridConsignItems.Rows[0].Cells["Weight"].Value = 0;
                radGridConsignItems.Rows[0].Cells["WeightChrg"].Value = 0;
                radGridConsignItems.Rows[0].Cells["Weight"].Value = 0;
                radGridConsignItems.Rows[0].Cells["Freight"].Value = 0;
                radGridConsignItems.Rows[0].Cells["Description"].Value = "";
                radGridConsignItems.Rows[0].Cells["Rate"].Value = 0;

                radGridConsignItems.Columns["GoodsValue"].ReadOnly = false;
                radGridConsignItems.Columns["Weight"].ReadOnly = false;
                radGridConsignItems.Columns["WeightChrg"].ReadOnly = false;
                radGridConsignItems.Columns["Weight"].ReadOnly = false;
                radGridConsignItems.Rows[0].Cells["WeightVolume"].ReadOnly = true;
                radGridConsignItems.Columns["MOP"].ReadOnly = false;
                radGridConsignItems.Columns["ConsignmentType"].ReadOnly = false;

                txtFreightChg.Text = "0";
                txtCartage.Text = "0";
                txtCartageDel.Text = "0";
                txtHamaliChg.Text = "0";
                txtInsurance.Text = "0";
                txtDocumentChg.Text = "0";
                txtServiceTax.Text = "0";
                txtAmount.Text = "0";
                txtCollCartageRemark.Text = "";
                txtComment.Text = "";
            }
            catch 
            { }

            if (chkFTLAssignCities.Checked)
            {
                try
                {
                    radGridConsignItems.Rows[0].Cells["ConsignmentType"].Value = strFTLTypeConsID;

                    DataTable dtConsignmentSettings = new DataView(dtConsignType, "ConsignmentSubTypeID1 = " + strFTLTypeConsID.Split('-')[0],
                                                    "", DataViewRowState.CurrentRows).ToTable();

                    if (dtConsignmentSettings != null && dtConsignmentSettings.Rows.Count > 0)
                    {
                        intIsMOPEditable = Convert.ToInt32(dtConsignmentSettings.Rows[0]["IsMOPEditable"].ToString());
                        intDefaultMOPId = Convert.ToInt32(dtConsignmentSettings.Rows[0]["DefaultMOPId"].ToString());
                        intBillingUnit = Convert.ToInt32(dtConsignmentSettings.Rows[0]["BillingUnit"].ToString());
                        intMinAllUnitChargeWeight = Convert.ToInt32(dtConsignmentSettings.Rows[0]["MinWeightAllUnits"].ToString());
                        intMinPerUnitChargeWeight = Convert.ToInt32(dtConsignmentSettings.Rows[0]["MinWeightPerUnit"].ToString());
                        intIsVolumetricWeight = Convert.ToInt32(dtConsignmentSettings.Rows[0]["IsVolumetricWeight"].ToString());
                    }
                }
                catch (Exception)
                {
                    intIsMOPEditable = 0;
                    intDefaultMOPId = 0;
                    intBillingUnit = 1;
                    intMinAllUnitChargeWeight = 0;
                    intMinPerUnitChargeWeight = 0;
                    intIsVolumetricWeight = 0;
                }

                try
                {
                    if (radGridConsignItems.Rows[0].Cells["ConsignmentType"].Value.ToString() == strFTLTypeConsID)
                    {
                        radGridConsignItems.Rows[0].Cells["MOP"].Value = intDefaultMOPId;
                        radGridConsignItems.Rows[0].Cells["IsVolumetricWeight"].Value = intIsVolumetricWeight;
                        radGridConsignItems.Rows[0].Cells["MinWeightAllUnits"].Value = intMinAllUnitChargeWeight;
                        radGridConsignItems.Rows[0].Cells["MinWeightPerUnit"].Value = intMinPerUnitChargeWeight;
                        radGridConsignItems.Rows[0].Cells["Rate"].Value = 0;
                        radGridConsignItems.Columns["MOP"].ReadOnly = true;
                        radGridConsignItems.Columns["ConsignmentType"].ReadOnly = true;
                    }
                }
                catch { }
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

        private void ResetFTLChanges()
        {
            _intPickupCityID = 0;
            _intDropOffCityID = 0;
            _pickupCityShortName = "";
            _dropoffCityShortName = "";

            if (HasFTLBooking)
            {
                lblFTL.Visible = true;
                chkFTLAssignCities.Checked = false;
                chkFTLAssignCities.Visible = true;
                lnkAssignedPickups.Text = "";
                lnkAssignedPickups.Visible = true;
            }
            else
            {
                lblFTL.Visible = false;
                chkFTLAssignCities.Checked = false;
                chkFTLAssignCities.Visible = false;
                lnkAssignedPickups.Text = "";
                lnkAssignedPickups.Visible = false;
            }
        }

        private void txtCartageDel_TextChanged(object sender, EventArgs e)
        {
            if(blnIsSTACargoCompany && intCompanyID == 3035)
                TotalAmount();
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

        private void txtBillNo_Leave(object sender, EventArgs e)
        {
            if (intCompanyID == 3034)   //Sainath Express 
                TotalAmount();
        }

        public void AssignTotalCartageAmount(int TotalOfBreakup, int PickupC, int CommC, int ReturnC)
        {
            _TotalCartage = TotalOfBreakup;
            _PickupCartage = PickupC;
            _CommCartage = CommC;
            _ReturnCartage = ReturnC;
        }

        private void txtCartage_Enter(object sender, EventArgs e)
        {
            try
            {
                if (intHasCartageBreakup == 1)
                {
                    CRS2011DeskApp.Cargo.frmCartageBreakup frm = new frmCartageBreakup(this, _PickupCartage,_CommCartage,_ReturnCartage);
                    frm.ShowDialog();

                    if (_TotalCartage >= 0)
                        txtCartage.Text = _TotalCartage.ToString();

                    txtCartageDel.Focus();
                }
                TotalAmount();
            }
            catch (Exception ex)
            { }
        }

        private void txtCartage_Leave(object sender, EventArgs e)
        {
            TotalAmount();
        }

        private void ResetCartageBreakups()
        {
            _TotalCartage = 0;
            _PickupCartage = 0;
            _CommCartage = 0;
            _ReturnCartage = 0;
        }

        private void SwapNameAndNoLables()
        {
            try
            {
                if (HasNoBeforeName == 1)
                {
                    Common.swapControlsInTable(lblMobileNo, lblNameSender);
                    Common.swapControlsInTable(panel4, tableLayoutPanel19);
                    Common.swapControlsInTable(lblMobileNoReceiver, lblNameReceiver);
                    Common.swapControlsInTable(panel3, txtNameReceiver);
                }
            }
            catch (Exception ex)
            { }

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

        private void pbCargoLogo_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.pbCargoLogo, "To display your company's logo, Click here.");
        }

        private void pbCargoLogo_Click(object sender, EventArgs e)
        {
            string chk = Common.GetCheckSumForWebLogin(intCompanyID, Common.GetUserID());
            if (chk != "")
            {
                string strPath = Common.GetBusCRSURL() + "/Luggage/TicketFormatLaserEXE?";
                //string strPath = "http://localhost:29563" + "/Luggage/TicketFormatLaserEXE?";
                ArrayList arr = new ArrayList();
                arr.Add("chk=" + chk);
                strPath += Common.GetReportQueryString(arr);
                System.Diagnostics.Process.Start(strPath);
            }
        }

        private void txtSearchPartySender_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
               if (dtPartyFrom != null && dtPartyFrom.Rows.Count > 0)
                {
                    DataView dv = dtPartyFrom.Copy().DefaultView;
                    //radDPartySender.DataSource = dtPartyFrom;
                    radDPartySender.DisplayMember = "Party";
                    radDPartySender.ValueMember = "PartyDetails";
                    dv.RowFilter = "Party LIKE '%" + txtSearchPartySender.Text + "%'";
                    DataTable dtFilteredBusList = dv.ToTable();

                    if (dtFilteredBusList.Rows.Count > 0)
                    {
                        radDPartySender.DataSource = dtFilteredBusList;
                        radDPartySender.SelectedIndex = 0;
                    }
                    else
                    {
                        radDPartySender.DataSource = null;
                    }
                    radDPartySender.Focus();
                }

                txtSearchPartySender.Focus();
            }
            catch (Exception ex)
            {
                radDPartySender.DisplayMember = "Party";
                radDPartySender.ValueMember = "PartyDetails";
                radDPartySender.DataSource = dtPartyFrom;
            }
        }

        private void txtSearchPartyReceiver_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (dtPartyTo != null && dtPartyTo.Rows.Count > 0)
                {
                    DataView dv = dtPartyTo.Copy().DefaultView;
                    //radDPartyReceiver.DataSource = dtPartyFrom;
                    radDPartyReceiver.DisplayMember = "Party";
                    radDPartyReceiver.ValueMember = "PartyDetails";
                    dv.RowFilter = "Party LIKE '%" + txtSearchPartyReceiver.Text + "%'";
                    DataTable dtFilteredBusList = dv.ToTable();

                    if (dtFilteredBusList.Rows.Count > 0)
                    {
                        radDPartyReceiver.DataSource = dtFilteredBusList;
                        radDPartyReceiver.SelectedIndex = 0;
                    }
                    else
                    {
                        radDPartyReceiver.DataSource = null;
                    }
                    radDPartyReceiver.Focus();
                }

                txtSearchPartyReceiver.Focus();
            }
            catch (Exception ex)
            {
                radDPartyReceiver.DisplayMember = "Party";
                radDPartyReceiver.ValueMember = "PartyDetails";
                radDPartyReceiver.DataSource = dtPartyTo;
            }
        }
    }
}
