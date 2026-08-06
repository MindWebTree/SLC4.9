$(document).ready(function () {
    /*$('.startdate').datetimepicker();
    $('.enddate').datetimepicker();*/
	$('.startdate').datetimepicker();
        $('.enddate').datetimepicker({
            useCurrent: false //Important! See issue #1075
        });
        $(".startdate").on("dp.change", function (e) {
            $('.enddate').data("DateTimePicker").minDate(e.date);
        });
        $(".enddate").on("dp.change", function (e) {
            $('.startdate').data("DateTimePicker").maxDate(e.date);
        });
    if ($(".date_sectiontoggle").attr("checked") == 'checked') {
        $("div#date_section-block").slideDown();
    }
    if ($(".toggleiconimage").attr("checked") == 'checked') {
        $("div#toggleIconSection").slideDown();
    }
    if ($(".popupappearstoggle").attr("checked") == 'checked') {
        $("table#add-campaign-block-table tr").remove();
        $("input#show-hide-website-element").prop('checked', true);

        /* Add Element START */
        var loop_tr = 0;
        $("table.dnamic_campaign_html tr").each(function () {
            var condition_id = $(this).find('.condition_id').html();
            var condition_url = $(this).find('.condition_url').html();
            $('#dropdownlist_campaign_rule option:selected').removeAttr('selected');
            $("#dropdownlist_campaign_rule select").val(condition_id).change();
            $("#dropdownlist_campaign_rule select option[value='" + condition_id + "']").attr("selected", "selected");
            var new_element = '<tr><td><div class="campaign-rule-container"><label>URL</label>';
            new_element += '<select name="campaign_rule" class="form-control campaign-rule" selected="' + condition_id + '">';
            new_element += $("#dropdownlist_campaign_rule select").html();
            new_element += '</select>';
            new_element += '<input type="text" name="campaign_rule_value" class="form-control campaign-rule-value" placeholder="http://yoursite.com/goods" value="' + condition_url + '">';
            if (loop_tr != '0') 
                new_element += '<button type="button" class="delete-campaign-rule btn btn-info">Remove</button></div></td></tr>';
            else 
                new_element += '<input style="width:250px;display:inline-block;margin-left: 10px;" class="icon-upload" type="file" id="FU_ImportURL" /><span class="import-url" id="Import_URL">Import</span>';
            $("table#add-campaign-block-table tbody").append(new_element);
            loop_tr++;
        });
        /* Add Element END */
        $("div#add-campaign-block").slideDown();
    } else {
        var campaign_val = $("#dropdownlist_campaign_rule select").html();
        $("select.form-control.campaign-rule").html(campaign_val);
    }

    $("li.preview-link a").click(function (e) {
        e.preventDefault();
        var html_div = $(this).attr("html_div");
        $('#myModalHTML').html($(html_div).html());
        $("#myModal").modal('show');
    });

});

//start to show alert on delete
       function confirmation() {

           if (confirm('are you sure you want to delete ?')) {
               return true;
           } else {
               return false;
           }
       }

//end to show alert on delete
        
function enablehome(id,page)
{
    id = $(id).attr('titleid');
    $('.togglecreate:checked').each(function () {
        if (id != $(this).attr('titleid')) {

            $(this).removeAttr("checked");
        }

    });


    //$(this).attr("checkedstatus", "True");
                
    $.ajax({
        type: "POST",
        url: "/bulvan/occationalpopsettings.aspx/homeenablecheck",
        data: '{id: "' + id + '",pageid:"'+page+'"}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
        },
        failure: function (response) {
            alert(response.d);
        }
    });
}

function showpop(id)
{
    debugger;
    $.ajax({
        type: "POST",
        url: "/bulvan/occationalpopsettings.aspx/GetPopupContent",
        data: '{campaignId: "' + id + '"}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: OnSuccess,
        failure: function (response) {
            alert(response.d);
        }
    });
    function OnSuccess(response) {
        debugger;
        var data = response.d;
        $('#myModal').html(data);
        $("#myModal").modal('show');
    }
}
      
      

$(document).ready(function () {
    $(".popupappearstoggle").change(function () {
        if ($(this).prop("checked") == true) {
            $("#add-campaign-block").slideDown();
        } else {
            $("#add-campaign-block").slideUp();
        }
    });
    $(".date_sectiontoggle").change(function () {
        if ($(this).prop("checked") == true) {
            $("#date_section-block").slideDown();
        } else {
            $("#date_section-block").slideUp();
        }
    });
    $(".toggleiconimage").change(function () {
        if ($(this).prop("checked") == true) {
            $("#toggleIconSection").slideDown();
        } else {
            $("#toggleIconSection").slideUp();
        }
    });
    $("#Import_URL").click(function () {
        var regex = /^([a-zA-Z0-9\s_\\.\-:])+(.csv|.txt)$/;
        if (regex.test($("#FU_ImportURL").val().toLowerCase())) {
            if (typeof (FileReader) != "undefined") {
                var reader = new FileReader();
                reader.onload = function (e) {
                    var rows = e.target.result.split("\n");
                    var dsdsad = $("table#add-campaign-block-table tbody .campaign-rule-value").attr('value');
                    if ($("table#add-campaign-block-table tbody .campaign-rule-value").attr('value')=='')
                    $("table#add-campaign-block-table tbody").html('');
                    for (var i = 0; i < rows.length; i++) {
                        if (i > 0 && rows[i] != '') {
                            var cells = rows[i].split(',');
                            var new_element = '<tr><td><div class="campaign-rule-container"><label>URL</label>';
                            for (var j = 0; j < cells.length; j++) {
                                if (j == 0) {
                                    new_element += '<select name="campaign_rule" class="form-control campaign-rule">';
                                    var options = "contains,does not contain,equals,does not equal";
                                    options = options.split(',');
                                    for (var option = 0; option < options.length; option++) {
                                        if (cells[j] == options[option])
                                            new_element += '<option selected="selected" value="' + (option + 1) + '">' + options[option] + '</option>';
                                        else
                                            new_element += '<option value="' + (option + 1) + '">' + options[option] + '</option>';
                                    }
                                    new_element += '</select>';

                                }
                                else {
                                    new_element += '<input type="text" name="campaign_rule_value" class="form-control campaign-rule-value" value="' + cells[j] + '">';
                                }

                            }
                            debugger;
                            if ($("table#add-campaign-block-table tbody .campaign-rule-value").attr('value') != undefined)
                                new_element += '<button type="button" class="delete-campaign-rule btn btn-info">Remove</button></div></td></tr>';
                            else if (i > 1)
                                new_element += '<button type="button" class="delete-campaign-rule btn btn-info">Remove</button></div></td></tr>';
                            else
                                new_element += '<input style="width:250px;display:inline-block;margin-left: 10px;" class="icon-upload" type="file" id="FU_ImportURL" /><span class="import-url" id="Import_URL">Import</span>';
                            $("table#add-campaign-block-table tbody").append(new_element);

                        }
                    }
                }
                reader.readAsText($("#FU_ImportURL")[0].files[0]);
            } else {
                alert("This browser does not support HTML5.");
            }
        } else {
            alert("Please upload a valid CSV file.");
        }

    });
    $("button.add-campaign-rule.btn.btn-info").click(function () {
        var new_element = '<tr><td><div class="campaign-rule-container"><label>URL</label>';
        new_element += '<select name="campaign_rule" class="form-control campaign-rule">';
        new_element += $("#dropdownlist_campaign_rule select").html();
        new_element += '</select>';
        new_element += '<input type="text" name="campaign_rule_value" class="form-control campaign-rule-value" placeholder="http://yoursite.com/goods" value="">';
        new_element += '<button type="button" class="delete-campaign-rule btn btn-info">Remove</button></div></td></tr>';
        $("table#add-campaign-block-table tbody").append(new_element);
    });
    $("body").on("click", "button.delete-campaign-rule.btn.btn-info", function () {
        $(this).parent().parent().parent().remove();
    });

    $("input#popupappearstoggle").prop('checked', false);
	
	if($('.report-panel-inner-wrapper .inline-pagination#pnReports-pagination >span').length){
    var pagination_html = $('.report-panel-inner-wrapper .inline-pagination#pnReports-pagination >span').html();	
    pagination_html = pagination_html.replace(/\&nbsp;/g, "");
    $('.report-panel-inner-wrapper .inline-pagination#pnReports-pagination >span').html(pagination_html);		
	}
	
	if($('.report-panel-inner-wrapper .inline-pagination#lvConversionReport-pagination >span').length){
    pagination_html = $('.report-panel-inner-wrapper .inline-pagination#lvConversionReport-pagination >span').html();
    pagination_html = pagination_html.replace(/\&nbsp;/g, "");
    $('.report-panel-inner-wrapper .inline-pagination#lvConversionReport-pagination >span').html(pagination_html);	
	}

	$("#full-website-element").change(function () {
	    if ($(this).prop("checked") == true) {
	        $(".popupappearstoggle").prop('checked', false);
	        $("#add-campaign-block").slideUp();
	    } else {
	        $("#add-campaign-block").slideUp();
	        $(".popupappearstoggle").prop('checked', true);
	    }
	});


	$("#show-hide-website-element").change(function () {
	    if ($(this).prop("checked") == true) {
	        $(".popupappearstoggle").prop('checked', true);
	        $("#add-campaign-block").slideDown();
	    } else {
	        $("#add-campaign-block").slideUp();
	        $(".popupappearstoggle").prop('checked', false);
	    }
	});
	
	
    /* Campaign Filter jQuery START */
	$("select#campaign-filter-status").change(function () {
	    var val = $(this).val();
	    if (val == "") {
	        $(".home-page-popup-count tr td").css("display", "block");
	    } else {
	        $(".home-page-popup-count tr td").css("display", "none");
	        $("." + val + "").each(function () {
                $(this).parent().css("display", "block");
	        });
	    }
	    var valname = $("input#campaign-filter-name").val();
	    if (valname != "") {
	        $(".campaign-name-check").each(function () {
	            var name = $(this).val();
	            //if (!name.startsWith(valname)) {
	            if (name.indexOf(valname)<0) {
	                $(this).parent().parent().css("display", "none");
	            }
	        });
	    }
	});
	$("input#campaign-filter-name").keyup(function () {
	    var val = $(this).val();
	    var valstatus = $("select#campaign-filter-status").val();
	    if (val == "") {
	        $(".home-page-popup-count tr td").css("display", "block");
	    } else {
	        $(".home-page-popup-count tr td").css("display", "none");
	        $(".campaign-name-check").each(function () {
	            var name = $(this).val();
	            //if (name.startsWith(val)) {
	            if (name.indexOf(val) >= 0) {
	                $(this).parent().parent().css("display", "block");
	            }
	        });
	    }

	    if (valstatus == "True-status") {
	        $(".False-status").each(function () {
	            $(this).parent().css("display", "none");
	        });
	    } else if (valstatus == "False-status") {
	        $(".True-status").each(function () {
	            $(this).parent().css("display", "none");
	        });
	    }
	});
    /* Campaign Filter jQuery END */
});




     
