import {HttpClient, HttpParams} from "@angular/common/http";
import {map} from "rxjs/operators";
import {PaginatedResult} from "../_models/Pagination";
import {UserParams} from "../_models/userParams";

export function getPaginatedResult<T>(url: string, params: HttpParams, http: HttpClient) {
  const paginationResult: PaginatedResult<T> = new PaginatedResult<T>()
  return http.get<T>(url, {observe: 'response', params}).pipe(
    map(response => {
      paginationResult.Result = response.body;
      if (response.headers.get('Pagination') !== null) {
        paginationResult.Pagination = JSON.parse(response.headers.get('Pagination'));
      }
      return paginationResult;
    }));
}

export function LoadPaginationParams(pageNumber: number, pageSize: number): HttpParams {
  let params = new HttpParams();
  params = params.append('pageNumber', pageNumber.toString()); //you can't just append its immutable
  params = params.append('pageSize', pageSize.toString());
  return params;
}
