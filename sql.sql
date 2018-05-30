DELIMITER $$

USE `crs2011`$$

DROP PROCEDURE IF EXISTS `spCargoGetVehicleNo_testpraj`$$

CREATE DEFINER=`root`@`%` PROCEDURE `spCargoGetVehicleNo_testpraj`(
    p_CompanyID INT,
    p_FromCityID INT,
    p_ToCityID INT,
    p_JourneyDate DATE,
    p_Mode INT
)
BEGIN
    DECLARE p_SharedBranchID INT;
    DECLARE p_SharedCompanyID INT;
    DECLARE p_DateBackOneYear DATETIME;
	
	IF p_Mode = 1 THEN
		/*
			changed on 29-07-2016 with Vikas: if mode = 1 then expect SharedCompany in ToCityID and SharedBranch in FromCityID
		*/
		SET p_SharedCompanyID = p_ToCityID; /* p_ToCityID is CompanyID */
		IF (p_FromCityID>0) THEN /* p_FromCityID is BranchID */
			SELECT CompanyId INTO p_SharedCompanyID FROM tblBranches WHERE branchid = p_FromCityID LIMIT 1;
		END IF;
		SET p_SharedCompanyID = COALESCE(p_SharedCompanyID,0);
		/*
			changed on 22-06-2015 with Dhaval: if mode = 1 then for all the companies, get buses which are C (cargo) or X (bus+cargo)
		*/
		
		IF p_CompanyID IN (156,184,322,805,2649,2921) THEN
			/* Non-Sharing Buses */
			IF p_CompanyID IN (156) THEN
				SELECT 
					DISTINCT BusNumber 'VehicleNo', BusNumber 'BusNumber',BusID 'VehicleID',BusMobileNo 'BusMobileNo'
				FROM tblbuses 
				WHERE companyid IN (p_CompanyID) AND BusOrCargo IN ('C','X') AND IsActive=1
				ORDER BY BusNumber;
			ELSE
				SELECT 
					DISTINCT BusNumber'VehicleNo', BusNumber 'BusNumber',BusID 'VehicleID',BusMobileNo  'BusMobileNo'
				FROM tblbuses 
				WHERE companyid IN (p_CompanyID,p_SharedCompanyID) AND BusOrCargo IN ('C','X') AND IsActive=1
				ORDER BY BusNumber;
			END IF;
		ELSEIF (p_CompanyID > 0) THEN
			SELECT 
				DISTINCT UPPER(LTRIM(RTRIM(BusNumber))) 'VehicleNo', UPPER(LTRIM(RTRIM(BusNumber))) 'BusNumber',BusID 'VehicleID',,BusMobileNo 'BusMobileNo'
			FROM tblbuses 
			WHERE companyid IN (p_CompanyID,p_SharedCompanyID) AND BusOrCargo IN ('C','X') AND IsActive=1
			ORDER BY LTRIM(RTRIM(BusNumber));
		ELSE
			/* THE BELOW CONDITION WOULD NEVER BE CALLED */
			IF p_CompanyID IN(1,66,805,756,11) THEN
				SELECT 
					DISTINCT UPPER(LTRIM(RTRIM(BusNumber))) 'VehicleNo', UPPER(LTRIM(RTRIM(BusNumber))) 'BusNumber',BusID 'VehicleID',BusMobileNo 'BusMobileNo'
				FROM tblbuses 
				WHERE companyid = p_CompanyID AND IsActive=1
				ORDER BY LTRIM(RTRIM(BusNumber));
			ELSE
				SELECT 
					DISTINCT UPPER(LTRIM(RTRIM(BusNumber))) 'VehicleNo', UPPER(LTRIM(RTRIM(BusNumber))) 'BusNumber',BS.BusID 'VehicleID',BS.BusMobileNo 
				FROM tblServiceSubRoutes SSR
				INNER JOIN tblbusschedule BSC ON BSC.TripID = SSR.TripID AND BSC.JourneyDate = SSR.JourneyDate
				INNER JOIN tblBuses BS ON BS.BusID = BSC.BusID AND BS.CompanyID = SSR.CompanyID
				WHERE SSR.companyid = p_CompanyID 
				AND BSC.journeydate = p_JourneyDate;
			END IF;
		END IF;
	ELSEIF p_Mode = 100 THEN /*Branch Transfered Vehicles*/
		SELECT 
			DISTINCT CONCAT(DM.VehicleNo ,' : ', DATE_FORMAT(DM.TransferDateTime,'%d-%b-%Y')) 'VehicleNo',
			DM.VehicleNo 'VehicleNoOnly',DM.VehicleID 'VehicleID'
		FROM tblCargoBranchTransferMaster DM
		INNER JOIN tblcargoBranchTransferdetails DT ON DT.BranchTransferID = DM.BranchTransferID
		INNER JOIN tblCargoBookings BKS ON BKS.BookingID = DT.BookingID
		WHERE DM.FromBranchID = p_FromCityID 	/*p_FromCityID uses as BranchID*/
		AND DM.ToBranchID = p_ToCityID /*p_ToBranchID uses as BranchID*/
		AND DT.IsDeleted = 0
		AND DT.IsReceived = 0
		AND BKS.CompanyID = p_CompanyID
		AND BKS.IsInMovement = 1
		AND BKS.IsBranchTransfered = 1
		ORDER BY DM.VehicleNo ASC;
	ELSEIF p_Mode = 200 THEN /*Branch Receipt Vehicle*/
		SELECT 
			DISTINCT DM.VehicleNo
		FROM tblCargoDispatchMaster DM
		INNER JOIN tblcargodispatchdetails DT ON DT.DispatchID = DM.DispatchID
		INNER JOIN tblCargoBookings BKS ON BKS.BookingID = DT.BookingID
		WHERE 
		-- DT.IsDeleted = 0
		-- and DT.IsReceived = 0
		BKS.CompanyID = p_CompanyID
		ORDER BY DM.VehicleNo ASC;
	ELSEIF p_Mode = 300 THEN /*Dispatched Vehicles for Bus Memo Voucher*/
		SELECT 
			DISTINCT LTRIM(RTRIM(DM.VehicleNo)) 'VehicleNo',DM.VehicleID 'VehicleID'
		FROM tblCargoDispatchMaster DM
		INNER JOIN tblcargodispatchdetails DT ON DT.DispatchID = DM.DispatchID
		INNER JOIN tblCargoBookings BKS ON BKS.BookingID = DT.BookingID
		WHERE DT.IsDeleted = 0
		AND DT.IsReceived = 0
		AND BKS.CompanyID = p_CompanyID
		AND BKS.IsInMovement = 1
		AND DM.DispatchDateTime BETWEEN CONCAT(p_JourneyDate,' 00:00:00') AND CONCAT(p_JourneyDate,' 23:59:59')
		ORDER BY DM.VehicleNo ASC;
		
	ELSEIF p_Mode = 400 THEN
	
		SET p_SharedCompanyID = p_ToCityID; /* p_ToCityID is CompanyID */
		IF (p_FromCityID>0) THEN /* p_FromCityID is BranchID */
			SELECT CompanyId INTO p_SharedCompanyID FROM tblBranches WHERE branchid = p_FromCityID LIMIT 1;
		END IF;
		SET p_SharedCompanyID = COALESCE(p_SharedCompanyID,0);
		/*
			changed on 22-06-2015 with Dhaval: if mode = 1 then for all the companies, get buses which are C (cargo) or X (bus+cargo)
		*/
		
		IF p_CompanyID IN (156,184,322,805,2649,2921) THEN
			/* Non-Sharing Buses */
			IF p_CompanyID IN (156) THEN
				SELECT 
					DISTINCT BusNumber 'VehicleNo', BusNumber 'BusNumber', `BusID` 'VehicleID',BusMobileNo 'BusMobileNo'
				FROM tblbuses 
				WHERE companyid IN (p_CompanyID) AND BusOrCargo IN ('C','X') AND IsActive=1
				ORDER BY BusNumber;
			ELSE
				SELECT 
					DISTINCT BusNumber'VehicleNo', BusNumber 'BusNumber',`BusID` 'VehicleID',BusMobileNo 'BusMobileNo'
				FROM tblbuses tb 
				WHERE companyid = p_CompanyID AND BusOrCargo IN ('C','X') AND IsActive=1
				UNION 
				SELECT 
					DISTINCT BusNumber'VehicleNo', BusNumber 'BusNumber',`BusID` 'VehicleID',tb.BusMobileNo
				FROM tblcargocompanysharing ccs
				INNER JOIN tblcargovehiclesharing t3 ON t3.SharedID = ccs.ID AND ccs.HasVehiclewise = 1 AND SharedWithCompanyID = p_SharedCompanyID
				INNER JOIN tblbuses tb ON tb.BusID = t3.VehicleID
				WHERE ccs.CompanyID = p_CompanyID
				AND tb.BusOrCargo IN ('C','X') AND tb.IsActive=1
				ORDER BY BusNumber;
			END IF;
		ELSEIF (p_CompanyID > 0) THEN
			SELECT 
				DISTINCT UPPER(LTRIM(RTRIM(BusNumber))) 'VehicleNo', UPPER(LTRIM(RTRIM(BusNumber))) 'BusNumber',`BusID` 'VehicleID',BusMobileNo 'BusMobileNo'
			FROM tblbuses 
			WHERE companyid = p_CompanyID AND BusOrCargo IN ('C','X') AND IsActive=1
			UNION
			SELECT 
				DISTINCT UPPER(LTRIM(RTRIM(BusNumber))) 'VehicleNo', UPPER(LTRIM(RTRIM(BusNumber))) 'BusNumber',`BusID` 'VehicleID',tb.BusMobileNo
			FROM tblcargocompanysharing ccs
			INNER JOIN tblcargovehiclesharing t3 ON t3.SharedID = ccs.ID AND ccs.HasVehiclewise = 1 AND SharedWithCompanyID = p_SharedCompanyID
			INNER JOIN tblbuses tb ON tb.BusID = t3.VehicleID
			WHERE ccs.CompanyID = p_CompanyID
			AND tb.BusOrCargo IN ('C','X') AND tb.IsActive=1
			ORDER BY VehicleNo;	
		ELSE
			/* THE BELOW CONDITION WOULD NEVER BE CALLED */
			IF p_CompanyID IN(1,66,805,756,11) THEN
				SELECT 
					DISTINCT UPPER(LTRIM(RTRIM(BusNumber))) 'VehicleNo', UPPER(LTRIM(RTRIM(BusNumber))) 'BusNumber',`BusID` 'VehicleID',tb.BusMobileNo
				FROM tblbuses 
				WHERE companyid = p_CompanyID AND IsActive=1
				ORDER BY LTRIM(RTRIM(BusNumber));
			ELSE
				SELECT 
					DISTINCT UPPER(LTRIM(RTRIM(BusNumber))) 'VehicleNo', UPPER(LTRIM(RTRIM(BusNumber))) 'BusNumber', BS.BusID 'VehicleID'
				FROM tblServiceSubRoutes SSR
				INNER JOIN tblbusschedule BSC ON BSC.TripID = SSR.TripID AND BSC.JourneyDate = SSR.JourneyDate
				INNER JOIN tblBuses BS ON BS.BusID = BSC.BusID AND BS.CompanyID = SSR.CompanyID
				WHERE SSR.companyid = p_CompanyID 
				AND BSC.journeydate = p_JourneyDate;
			END IF;
		END IF;
		
	
	
	-- ELSE 
	ELSEIF p_Mode = 2 THEN /*Dispatched Vehicles*/
		SET p_DateBackOneYear = DATE_ADD(current_datepart(),INTERVAL -1 YEAR);
		
		SELECT 
			-- distinct concat(DM.VehicleNo ,' : ', date_format(DM.DispatchDateTime,'%d-%b-%Y')) 'VehicleNo',
			DISTINCT CASE WHEN COALESCE(BE.ChallanNo,0) = 0 THEN 
				CONCAT(DM.VehicleNo ,' : ', DATE_FORMAT(DM.DispatchDateTime,'%d-%b-%Y'))
			ELSE
				CONCAT(DM.VehicleNo ,' : ', DATE_FORMAT(DM.DispatchDateTime,'%d-%b-%Y'),' : ', COALESCE(BE.ChallanNo,0) ) 
			END 'VehicleNo',
			DM.VehicleNo 'VehicleNoOnly',
			DM.VehicleID
		FROM tblCargoDispatchMaster DM
		INNER JOIN tblcargodispatchdetails DT ON DT.DispatchID = DM.DispatchID
		INNER JOIN tblCargoBookings BKS ON BKS.BookingID = DT.BookingID
		LEFT JOIN tblCargoBookingsextended BE ON BE.BookingID = DT.BookingID
		WHERE DM.FromCityID = p_FromCityID 	
			AND DM.ToCityID = p_ToCityID
			AND DT.IsDeleted = 0
			AND DT.IsReceived = 0
			AND BKS.CompanyID = p_CompanyID
			AND BKS.IsInMovement = 1
			AND BKS.BookingDateTime>= p_DateBackOneYear
		/* prateek: 18-dec-2015: cargo-sharing */
		UNION  
		SELECT 
			-- distinct concat(DM.VehicleNo ,' : ', date_format(DM.DispatchDateTime,'%d-%b-%Y')) 'VehicleNo',
			DISTINCT CASE WHEN COALESCE(BE.ChallanNo,0) = 0 THEN 
				CONCAT(DM.VehicleNo ,' : ', DATE_FORMAT(DM.DispatchDateTime,'%d-%b-%Y'))
			ELSE
				CONCAT(DM.VehicleNo ,' : ', DATE_FORMAT(DM.DispatchDateTime,'%d-%b-%Y'),' : ', COALESCE(BE.ChallanNo,0) ) 
			END 'VehicleNo',
			DM.VehicleNo 'VehicleNoOnly',
			DM.VehicleID
		FROM tblCargoDispatchMaster DM
		INNER JOIN tblcargodispatchdetails DT ON DT.DispatchID = DM.DispatchID
		INNER JOIN tblCargoBookings BKS ON BKS.BookingID = DT.BookingID
		LEFT JOIN tblCargoBookingsextended BE ON BE.BookingID = DT.BookingID
		/*inner join tblCargoCompanySharing sh on sh.SharedWithCompanyID = p_CompanyID and dm.CompanyID = sh.CompanyID*/
		INNER JOIN tblbranches br ON br.branchid = dm.ToBranchID
		WHERE DM.FromCityID = p_FromCityID 	
			AND DM.ToCityID = p_ToCityID
			AND DT.IsDeleted = 0
			AND DT.IsReceived = 0
			AND BKS.IsInMovement = 1
			AND BKS.BookingDateTime>= p_DateBackOneYear
			AND br.CompanyID = p_CompanyID
		ORDER BY VehicleNo ASC;
	END IF;
END$$

DELIMITER ;