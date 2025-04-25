export const formatDate = (dateString: string | undefined): string => {  
   if (!dateString) {  
       return "N/A";  
   }  
   return new Date(dateString).toLocaleString("nl-NL", {  
       day: "2-digit",  
       month: "2-digit",  
       year: "numeric",  
   });  
};
