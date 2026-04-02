// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

(function () {
  var defaultTooltipMessage = "copy to clipboard";

  function createCopyIcon() {
    var icon = document.createElement("span");
    icon.className = "copy-detail-icon";
    icon.setAttribute("aria-hidden", "true");
    icon.innerHTML =
      '<svg viewBox="0 0 24 24" focusable="false"><path fill="currentColor" d="M16 1H6C4.9 1 4 1.9 4 3V17H6V3H16V1ZM19 5H10C8.9 5 8 5.9 8 7V21C8 22.1 8.9 23 10 23H19C20.1 23 21 22.1 21 21V7C21 5.9 20.1 5 19 5ZM19 21H10V7H19V21Z"></path></svg>';

    return icon;
  }

  function setTooltipText(button, text) {
    button.setAttribute("title", text);
    button.setAttribute("data-original-title", text);
  }

  function copyText(text) {
    if (navigator.clipboard && window.isSecureContext) {
      return navigator.clipboard.writeText(text);
    }

    return new Promise(function (resolve, reject) {
      var textArea = document.createElement("textarea");
      textArea.value = text;
      textArea.setAttribute("readonly", "");
      textArea.style.position = "fixed";
      textArea.style.top = "-9999px";

      document.body.appendChild(textArea);
      textArea.focus();
      textArea.select();

      try {
        if (document.execCommand("copy")) {
          resolve();
        } else {
          reject(new Error("Copy command was unsuccessful."));
        }
      } catch (error) {
        reject(error);
      } finally {
        document.body.removeChild(textArea);
      }
    });
  }

  function initializeTooltip(button) {
    if (!window.jQuery || !window.jQuery.fn || !window.jQuery.fn.tooltip) {
      return null;
    }

    var $button = window.jQuery(button);
    $button.tooltip({
      placement: "top",
      trigger: "hover focus"
    });

    return $button;
  }

  function initializeCopyableDetailFields() {
    var fields = document.querySelectorAll(".detail-fields dd");

    if (!fields.length) {
      return;
    }

    fields.forEach(function (field) {
      if (field.dataset.copyReady === "true") {
        return;
      }

      var copyValue = (field.innerText || field.textContent || "").trim();
      if (!copyValue) {
        return;
      }

      field.dataset.copyReady = "true";

      var wrapper = document.createElement("span");
      wrapper.className = "copy-detail-wrap";

      var text = document.createElement("span");
      text.className = "copy-detail-text";

      while (field.firstChild) {
        text.appendChild(field.firstChild);
      }

      var button = document.createElement("button");
      button.type = "button";
      button.className = "btn btn-link copy-detail-button";
      button.setAttribute("aria-label", "Copy to clipboard");
      setTooltipText(button, defaultTooltipMessage);
      button.appendChild(createCopyIcon());

      var $tooltipButton = initializeTooltip(button);

      button.addEventListener("click", function (event) {
        event.preventDefault();

        copyText(copyValue)
          .then(function () {
            setTooltipText(button, "copied!");

            if ($tooltipButton) {
              $tooltipButton.tooltip("show");
            }

            window.clearTimeout(button.copyResetTimer);
            button.copyResetTimer = window.setTimeout(function () {
              setTooltipText(button, defaultTooltipMessage);

              if ($tooltipButton) {
                $tooltipButton.tooltip("hide");
              }
            }, 2000);
          })
          .catch(function () {
            setTooltipText(button, "copy failed");

            if ($tooltipButton) {
              $tooltipButton.tooltip("show");
            }
          });
      });

      button.addEventListener("blur", function () {
        if (button.getAttribute("data-original-title") !== "copied!") {
          setTooltipText(button, defaultTooltipMessage);
        }
      });

      wrapper.appendChild(text);
      wrapper.appendChild(button);
      field.appendChild(wrapper);
    });
  }

  document.addEventListener("DOMContentLoaded", initializeCopyableDetailFields);
})();
