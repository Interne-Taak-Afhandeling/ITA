export const formatDate = (dateString: string | number | Date | null | undefined): string => {  
   if (!dateString) {  
       return "N.v.t";  
   }  
   return new Date(dateString).toLocaleString("nl-NL", {  
       day: "2-digit",  
       month: "2-digit",  
       year: "numeric",  
   });  
};
