const Status = {
    PLACED: { value: "PLACED", bg: "#674ea7" },
    ALLOCATING: { value: "ALLOCATING", bg: "" },
    INVOICING: { value: "INVOICING", bg: "" },
    PENDING_PAYMENT: { value: "PENDING_PAYMENT", bg: "" },
    PAID: { value: "PAID", bg: "#f8f9fa" },
    PICKUP: { value: "PICKUP", bg: "#3e84c6" },
    SHIPPING: { value: "SHIPPING", bg: "#cfe2f3" },
    SHIPPED: { value: "SHIPPED", bg: "" },
    SHIP_PAY_PENDING: { value: "SHIP_PAY_PENDING", bg: "#cc0000" },
    COMPLETE: { value: "COMPLETE", bg: "#a64d79" },
    HIDE: { value: "HIDE", bg: "#e69137" }
  };
  
  function onOpen() {
    const ui = SpreadsheetApp.getUi();
    ui.createMenu("🛠 Order Actions")
      .addItem("❌ Cancel Order", "handleCancel")
      .addItem("🔄 Un-Cancel Order", "handleUncancel")
      .addToUi();
  }
  
  function updateRowOrderStatus(sheet, row, newStatus) {
    const headers = sheet.getRange(1, 1, 1, sheet.getLastColumn()).getValues()[0];
    const colIndex = headers.indexOf("Status") + 1;
    if (colIndex < 1) {
      SpreadsheetApp.getUi().alert("🚫 Column 'Status' not found in headers");
      return;
    }
  
    try {
      sheet.getRange(row, colIndex).setValue(newStatus.value);
      sheet.getRange(row, colIndex).setBackground(newStatus.bg);
    } catch (e) {
      Logger.log(`❌ Failed to update cell: ${e}`);
    }
  }
   
  function handleUncancel() {
    const ss = SpreadsheetApp.getActiveSpreadsheet();
    const trackingSheet = ss.getSheetByName('New Tracking Sheet');
    const cancelledSheet = ss.getSheetByName('Cancelled Orders');
   
    const selectedRow = ss.getActiveRange().getRow();
    if (selectedRow === 1) {
      SpreadsheetApp.getUi().alert("🚫 Cannot un-cancel the header row.");
      return;
    }
   
    const headersCancelled = cancelledSheet.getRange(1, 1, 1, cancelledSheet.getLastColumn()).getValues()[0];
    const rowDataCancelled = cancelledSheet.getRange(selectedRow, 1, 1, cancelledSheet.getLastColumn()).getValues()[0];
   
    const rowNumberIndexCancelled = headersCancelled.indexOf("Row Number");
    if (rowNumberIndexCancelled === -1) {
      SpreadsheetApp.getUi().alert("⚠️ 'Row Number' column not found in Cancelled Orders sheet.");
      return;
    }
   
    const targetRowNumber = parseInt(rowDataCancelled[rowNumberIndexCancelled]);
    if (isNaN(targetRowNumber)) {
      SpreadsheetApp.getUi().alert("⚠️ Invalid 'Row Number' value.");
      return;
    }
   
    // Find the actual row index in Tracking sheet that matches the "Row Number"
    const headersTracking = trackingSheet.getRange(1, 1, 1, trackingSheet.getLastColumn()).getValues()[0];
    const rowNumberIndexTracking = headersTracking.indexOf("Row Number");
    if (rowNumberIndexTracking === -1) {
      SpreadsheetApp.getUi().alert("⚠️ 'Row Number' column not found in Tracking sheet.");
      return;
    }
   
    const dataRangeTracking = trackingSheet.getRange(2, 1, trackingSheet.getLastRow() - 1, trackingSheet.getLastColumn());
    const dataTracking = dataRangeTracking.getValues();
   
    let actualRowIndexInSheet = null;
   
    for (let i = 0; i < dataTracking.length; i++) {
      const row = dataTracking[i];
      const rowNumValue = parseInt(row[rowNumberIndexTracking]);
      if (rowNumValue === targetRowNumber) {
        actualRowIndexInSheet = i + 2; // +2 because header row skipped and cell rows index starting at 1
        break;
      }
    }
   
    if (!actualRowIndexInSheet) {
      SpreadsheetApp.getUi().alert(`❌ Could not find a matching row in Tracking sheet with 'Row Number' = ${targetRowNumber}`);
      return;
    }
   
    // Unhide the matched row
    trackingSheet.showRows(actualRowIndexInSheet);
   
    // Remove the row from Cancelled sheet
    cancelledSheet.deleteRow(selectedRow);
   
    SpreadsheetApp.getUi().alert(`✅ Order with Row Number ${targetRowNumber} was restored and made visible in Tracking sheet.`);
  }
   
   
  function handleCancel() {
    try {
      const ss = SpreadsheetApp.getActiveSpreadsheet();
      const trackingSheet = ss.getSheetByName('New Tracking Sheet');
      const cancelledSheet = ss.getSheetByName('Cancelled Orders');
   
      const row = ss.getActiveRange().getRow();
      if (row === 1) {
        SpreadsheetApp.getUi().alert("🚫 Cannot cancel the header row.");
        return;
      }
    
      const headers = trackingSheet.getRange(1, 1, 1, trackingSheet.getLastColumn()).getValues()[0];
      const trackingData = trackingSheet.getRange(row, 1, 1, trackingSheet.getLastColumn());
      const rowData = trackingData.getValues()[0];
    
      const reason = promptForReason();
      if (!reason) return;
  
      // Hide the row in Tracking sheet
      trackingSheet.hideRows(row);
    
      // Move rowData to Cancelled sheet
      cancelledSheet.appendRow(rowData);
      const lastRow = cancelledSheet.getLastRow();
      trackingData.copyTo(cancelledSheet.getRange(lastRow, 1), {contentsOnly: false});
      updateRowOrderStatus(cancelledSheet, lastRow, Status.HIDE);
  
      // Delete the row in Tracking sheet
      // trackingSheet.deleteRow(row);
  
      // Notify discord
      notifyBot(headers, rowData, reason);
    } catch (err) {
      Logger.log("Error handling order cancellation: " + err.message);
    }
  }
   
  function promptForReason() {
    const ui = SpreadsheetApp.getUi();
    const reasons = ['Distro out of stock', 'Distro price changed', 'Customer asked to cancel', 'Distro could not fulfill the order', 'OTHER'];
    const optionsText = reasons.map((r, i) => `${i + 1}. ${r}`).join('\n');
   
    const response = ui.prompt(
      "Select a cancellation reason by number:",
      optionsText,
      ui.ButtonSet.OK_CANCEL
    );
   
    if (response.getSelectedButton() !== ui.Button.OK) return null;
   
    const index = parseInt(response.getResponseText().trim(), 10);
    if (isNaN(index) || index < 1 || index > reasons.length) {
      ui.alert("⚠️ Invalid selection. Try again.");
      return promptForReason();
    }
   
    return reasons[index - 1];
  }
   
  function notifyBot(headers, rowData, reason) {
    const payload = {
      Reason: reason,
      Data: headers.reduce((acc, header, i) => {
        acc[header] = String(rowData[i]);
        return acc;
      }, {})
    };
   
    const url = 'https://f0ad-2601-600-8e83-a0c0-a1ed-a486-22f6-624e.ngrok-free.app/webhook/orderaction';
  
  
    UrlFetchApp.fetch(url, {
      method: 'post',
      contentType: 'application/json',
      payload: JSON.stringify(payload)
    });
  }  