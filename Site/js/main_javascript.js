
function myFunction() {
    var x = document.getElementById("myNavbar");
    if (x.className === "navbar") {
        x.className += " responsive";
    } else {
        x.className = "navbar";
    }
}


window.addEventListener("scroll", stickymyNavbar);
window.addEventListener("scroll", scrollprogress);

var myNavbar = document.getElementById("myNavbar");
var sticky = myNavbar.offsetTop;

function stickymyNavbar() {
  if (window.pageYOffset >= sticky) {
    myNavbar.classList.add("sticky")
  } else {
    myNavbar.classList.remove("sticky");
  }
}


function scrollprogress() {
  var winScroll = document.body.scrollTop || document.documentElement.scrollTop;
  var height = document.documentElement.scrollHeight - document.documentElement.clientHeight;
  var scrolled = (winScroll / height) * 100;
  document.getElementById("myBar").style.width = scrolled + "%";
}


var acc = document.getElementsByClassName("list_cat");
var i;
var h = document.getElementsByClassName("container");

for (i = 0; i < acc.length; i++) {
  acc[i].addEventListener("click", function() {
    this.classList.toggle("active");
    var panel = this.nextElementSibling;
    if (panel.style.maxHeight){
      panel.style.maxHeight = null;
    } else {
      panel.style.maxHeight = panel.scrollHeight + "px";
	  
    } 
  });
}
