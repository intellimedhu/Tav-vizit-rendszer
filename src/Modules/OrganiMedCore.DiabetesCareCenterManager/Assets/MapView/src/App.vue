<template>
  <div id="organimedcore-diabetes-care-center-manager-map-view">
    <div class="row form-group" v-if="!loading">
      <div class="col-lg-5 mb-3 mb-lg-0">
        <div class="card">
          <div class="card-header py-2 px-3">
            <h5 class="m-0">Megjelenített szakellátóhelyek:</h5>
          </div>
          <div class="card-body p-3">
            <table class="table table-sm table-borderless mb-2">
              <tbody>
                <tr>
                  <td class="p-0">
                    <div class="custom-control custom-switch">
                      <input type="radio" id="cp-01" name="cptype" class="custom-control-input" value="0,1" checked @change="setSelectedCenterTypes($event, '|')" />
                      <label class="custom-control-label" for="cp-01">
                        Felnőtt és gyermek szakellátóhely összesen
                      </label>
                    </div>
                  </td>
                  <td class="p-0 text-right w-25">{{ centerTypeStatistics['0,1'] || 0 }} db</td>
                </tr>

                <tr>
                  <td class="p-0">
                    <div class="custom-control custom-switch">
                      <input type="radio" id="cp-0" name="cptype" class="custom-control-input" value="0" @change="setSelectedCenterTypes($event, '&')" />
                      <label class="custom-control-label" for="cp-0">
                        Felnőtt szakellátóhely összesen
                      </label>
                    </div>
                  </td>
                  <td class="p-0 text-right w-25">{{ centerTypeStatistics['0'] || 0 }} db</td>
                </tr>

                <tr>
                  <td class="p-0">
                    <div class="custom-control custom-switch">
                      <input type="radio" id="cp-02" name="cptype" class="custom-control-input" value="0,2" @change="setSelectedCenterTypes($event, '&')" />
                      <label class="custom-control-label ml-3" for="cp-02">
                        Felnőtt és gesztációs szakellátóhely
                      </label>
                    </div>
                  </td>
                  <td class="p-0 text-right w-25">{{ centerTypeStatistics['0,2'] || 0 }} db</td>
                </tr>
                <tr>
                  <td class="p-0">
                    <div class="custom-control custom-switch">
                      <input type="radio" id="cp-03" name="cptype" class="custom-control-input" value="0,3" @change="setSelectedCenterTypes($event, '&')" />
                      <label class="custom-control-label ml-3" for="cp-03">
                        Felnőtt és folyamatos inzulinadagoló szakellátóhely
                      </label>
                    </div>
                  </td>
                  <td class="p-0 text-right w-25">{{ centerTypeStatistics['0,3'] || 0 }} db</td>
                </tr>
                <tr>
                  <td class="p-0">
                    <div class="custom-control custom-switch">
                      <input type="radio" id="cp-023" name="cptype" class="custom-control-input" value="0,2,3" @change="setSelectedCenterTypes($event, '&')" />
                      <label class="custom-control-label ml-3" for="cp-023">
                        Felnőtt, gesztációs és folyamatos inzulinadagoló szakellátóhely
                      </label>
                    </div>
                  </td>
                  <td class="p-0 text-right w-25">{{ centerTypeStatistics['0,2,3'] || 0 }} db</td>
                </tr>
                <tr>
                  <td class="p-0">
                    <div class="custom-control custom-switch">
                      <input type="radio" id="cp-1" name="cptype" class="custom-control-input" value="1" @change="setSelectedCenterTypes($event, '&')" />
                      <label class="custom-control-label" for="cp-1">
                        Gyermek szakellátóhely összesen
                      </label>
                    </div>
                  </td>
                  <td class="p-0 text-right w-25">{{ centerTypeStatistics['1'] || 0 }} db</td>
                </tr>
                <tr>
                  <td class="p-0">
                    <div class="custom-control custom-switch">
                      <input type="radio" id="cp-13" name="cptype" class="custom-control-input" value="1,3" @change="setSelectedCenterTypes($event, '&')" />
                      <label class="custom-control-label ml-3" for="cp-13">
                        Gyermek és folyamatos inzulinadagoló szakellátóhely
                      </label>
                    </div>
                  </td>
                  <td class="p-0 text-right w-25">{{ centerTypeStatistics['1,3'] }} db</td>
                </tr>
              </tbody>
            </table>

            <hr class="my-2" />

            <div class="row">
              <div class="col-12">
                <table class="table table-sm table-borderless mb-2">
                  <tbody>
                    <tr v-for="statusCode in accreditationStatusCodes" :key="statusCode.status">
                      <td class="p-0">
                        <div class="custom-control custom-switch">
                          <input type="checkbox" class="custom-control-input" :id="`checkbox-accreditation-status-${statusCode.status}`"
                                v-model="statusCode.selected"
                                @change="initMap" />
                          <label class="custom-control-label" :for="`checkbox-accreditation-status-${statusCode.status}`">
                            {{ statusCode.text }}
                          </label>
                        </div>
                      </td>
                      <td class="p-0 text-right">
                        {{ statusCode.count }} db
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>

            <hr class="my-2" />

            <div class="row">
              <label for="profile-search" class="col-12 col-form-label">
                Keresés, szűkítés:
              </label>
              <div class="col-12">
                <input type="search" id="profile-search" class="form-control" />
                <small class="text-muted">
                  <i class="fas fa-info-circle"></i>
                  Kereshet a szakellátóhely nevére, vezetőjére, címére, megyére
                </small>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="col-lg-7 pl-lg-0">
        <div class="card">
          <div class="card-body p-0">
            <div id="center-profiles-map" style="height:550px;"></div>
          </div>
          <div class="card-footer">
            <h5 class="m-0 text-center">
              Megjelenítve: {{ filteredCenterProfiles.length }} db
            </h5>
          </div>
        </div>
      </div>
    </div>

    <div class="row" v-if="!loading">
      <div class="col-12" v-show="filteredCenterProfiles.length">
        <hr>
        <div class="table-responsive">
          <b-table :items="filteredCenterProfiles"
                   class="border-bottom table-separated table-v-top mb-3"
                   :fields="tableFields"
                   
                   striped
                   small
                   thead-class="thead-half-dark"
                   :current-page="pagination.currentPage"
                   :per-page="pagination.limit">
            <template slot="accreditationStatus" slot-scope="data">
              <i :class="
                {
                  'fas fa-check text-success': data.item.accreditationStatus == 0,
                  'fas fa-check text-primary': data.item.accreditationStatus == 1,
                  'fas fa-minus-square text-warning': data.item.accreditationStatus == 2,
                  'fas fa-file-alt text-secondary': data.item.accreditationStatus == 3
                }"
                :title="accreditationStatusCodesMap[data.item.accreditationStatus]"></i>
                <small>
                  {{ accreditationStatusCodesMap[data.item.accreditationStatus] }}
                  <span class="d-block" title="Érvényben lévő minősítés megszerzésének dátuma">
                    {{ data.item.accreditationStatusDateFormat }}
                  </span>
                </small>
            </template>

            <template slot="centerName" slot-scope="data">
              <a href="#" data-toggle="modal" data-target="#center-profile-details-modal" @click="loadProfile(data.item.contentItemId)">
                <span v-html="highlightMatches(data.item.centerName)"></span>
              </a>
            </template>

            <template slot="centerFullAddress" slot-scope="data">
              <span v-html="highlightMatches(data.item.centerFullAddress)"></span>
            </template>

            <template slot="leader.fullName" slot-scope="data">
              <span v-html="highlightMatches(data.item.leader.fullName)"></span>
            </template>
            
            <template slot="centerTypes" slot-scope="data">
              <span class="d-block" v-for="type in data.item.centerTypeNames" :key="type">{{type}}</span>
            </template>

            <template slot="renewalCenterProfileStatus" slot-scope="data">
              <div class="text-center">
                <div v-if="authenticated && (isRenewalPeriod || data.item.underReview)">
                  <span v-if="data.item.renewalCenterProfileStatus == null">
                      <small class="d-block">
                        {{ centerProfileStatusCodes.empty[0] }}
                      </small>
                      <span class="text-nowrap">
                        <i class="far fa-square text-secondary" :title="centerProfileStatusCodes.empty[0]"></i>
                        <i class="far fa-square text-secondary" :title="centerProfileStatusCodes.empty[1]"></i>
                        <i class="far fa-square text-secondary" :title="centerProfileStatusCodes.empty[2]"></i>
                        <i class="far fa-square text-secondary" :title="centerProfileStatusCodes.empty[3]"></i>
                      </span>
                  </span>
                  <span v-if="data.item.renewalCenterProfileStatus === 0">
                      <small class="d-block">
                        {{centerProfileStatusCodes.current[0]}}
                      </small>
                      <span class="text-nowrap">
                        <i class="fas fa-square text-warning" :title="centerProfileStatusCodes.current[0]"></i>
                        <i class="far fa-square text-secondary" :title="centerProfileStatusCodes.empty[1]"></i>
                        <i class="far fa-square text-secondary" :title="centerProfileStatusCodes.empty[2]"></i>
                        <i class="far fa-square text-secondary" :title="centerProfileStatusCodes.empty[3]"></i>
                      </span>
                  </span>
                  <span :title="centerProfileStatusCodes[1]" v-if="data.item.renewalCenterProfileStatus === 1">
                      <small class="d-block">
                        {{centerProfileStatusCodes.current[1]}}
                      </small>
                      <span class="text-nowrap">
                        <i class="fas fa-check-square text-success" :title="centerProfileStatusCodes.filled[0]"></i>
                        <i class="fas fa-square text-warning" :title="centerProfileStatusCodes.current[1]"></i>
                        <i class="far fa-square text-secondary" :title="centerProfileStatusCodes.empty[2]"></i>
                        <i class="far fa-square text-secondary" :title="centerProfileStatusCodes.empty[3]"></i>
                      </span>
                  </span>
                  <span :title="centerProfileStatusCodes[2]" v-if="data.item.renewalCenterProfileStatus === 2">
                      <small class="d-block">
                        {{centerProfileStatusCodes.current[2]}}
                      </small>
                      <span class="text-nowrap">
                        <i class="fas fa-check-square text-success" :title="centerProfileStatusCodes.filled[0]"></i>
                        <i class="fas fa-check-square text-success" :title="centerProfileStatusCodes.filled[1]"></i>
                        <i class="fas fa-square text-warning" :title="centerProfileStatusCodes.current[2]"></i>
                        <i class="far fa-square text-secondary" :title="centerProfileStatusCodes.empty[3]"></i>
                      </span>
                  </span>
                  <span :title="centerProfileStatusCodes[3]" v-if="data.item.renewalCenterProfileStatus === 3">
                      <small class="d-block">
                        {{centerProfileStatusCodes.current[3]}}
                      </small>
                      <span class="text-nowrap">
                        <i class="fas fa-check-square text-success" :title="centerProfileStatusCodes.filled[0]"></i>
                        <i class="fas fa-check-square text-success" :title="centerProfileStatusCodes.filled[1]"></i>
                        <i class="fas fa-check-square text-success" :title="centerProfileStatusCodes.filled[2]"></i>
                        <i class="fas fa-square text-warning" :title="centerProfileStatusCodes.current[3]"></i>
                      </span>
                  </span>
                  <span :title="centerProfileStatusCodes[4]" v-if="data.item.renewalCenterProfileStatus === 4">
                      <small class="d-block">
                        {{centerProfileStatusCodes.filled[4]}}
                      </small>
                      <span class="text-nowrap">
                        <i class="fas fa-check-square text-success" :title="centerProfileStatusCodes.filled[0]"></i>
                        <i class="fas fa-check-square text-success" :title="centerProfileStatusCodes.filled[1]"></i>
                        <i class="fas fa-check-square text-success" :title="centerProfileStatusCodes.filled[2]"></i>
                        <i class="fas fa-check-square text-success" :title="centerProfileStatusCodes.filled[4]"></i>
                      </span>
                  </span>
                </div>
                <div class="text-center" v-if="authenticated && !isRenewalPeriod && !data.item.underReview">
                  <small class="text-muted">Jelenleg nincs felülvizsgálat</small>
                </div>
              </div>
            </template>
          </b-table>
        </div>

        <b-pagination v-model="pagination.currentPage"
                      align="center"
                      :total-rows="filteredCenterProfiles.length"
                      :per-page="pagination.limit"
                      :limit="9"
                      aria-controls="my-table"></b-pagination>
      </div>

      <div class="col-12 mt-3" v-show="!filteredCenterProfiles.length">
        <p class="text-muted text-center">
          {{ texts.noResult }}
        </p>
      </div>
    </div>

    <div class="row" v-show="loading">
      <div class="col">
        <div class="jumbotron m-0 p-5">
          <div class="d-flex justify-content-center">
            <div class="spinner-border text-secondary" role="status">
              <span class="sr-only"></span>
            </div>
          </div>
          <p class="lead text-center m-0">
            <i class="fas fa-clock"></i>
            Kérem várjon, az adatok betöltése folyamatban... 
          </p>
        </div>
      </div>
    </div>

    <div class="modal fade" id="center-profile-details-modal" tabindex="-1" role="dialog" :aria-labelledby="texts.modalTitle" aria-hidden="true">
      <div class="modal-dialog modal-lg" role="document">
        <div class="modal-content">
          <div class="modal-header">
            <h4 class="modal-title">
              {{ texts.modalTitle }}
            </h4>
            <button type="button" class="close" data-dismiss="modal" :aria-label="texts.close">
              <span aria-hidden="true">&times;</span>
            </button>
          </div>
          <div class="modal-body" id="modal-body-center-profile">
            <ul class="row list-unstyled" v-if="selectedCenterProfile">
              <li class="col-sm-3 text-sm-right">
                <strong>{{ texts.centerProfileName }}:</strong>
              </li>
              <li class="col-sm-9">
                {{ selectedCenterProfile.centerName }}
              </li>

              <li class="col-sm-3 text-sm-right">
                <strong>{{ texts.address }}:</strong>
              </li>
              <li class="col-sm-9">
                <address class="m-0">
                  <a :href="`http://www.google.com/maps/place/${selectedCenterProfile.centerLatitude},${selectedCenterProfile.centerLongitude}`"
                     target="_blank">
                    {{ selectedCenterProfile.centerZipCode }}
                    {{ selectedCenterProfile.centerSettlement }},
                    {{ selectedCenterProfile.centerAddress }}
                  </a>
                </address>
              </li>

              <li class="col-sm-3 text-sm-right">
                <strong>{{ texts.centerLeader }}:</strong>
              </li>
              <li class="col-sm-9">
                {{ selectedCenterProfile.leader.fullName }}
              </li>

              <li class="col-sm-3 text-sm-right">
                <strong>{{ texts.centerType }}:</strong>
              </li>
              <li class="col-sm-9">
                {{ selectedCenterProfile.centerTypeNames.join(', ') }}
              </li>

              <li class="col-sm-3 text-sm-right" v-if="authenticated && selectedCenterProfile.territorialRapporteur">
                <strong>{{ texts.territorialRapporteur }}:</strong>
              </li>
              <li class="col-sm-9" v-if="authenticated && selectedCenterProfile.territorialRapporteur">
                {{ selectedCenterProfile.territorialRapporteur }}
              </li>

              <li class="col-sm-3 mt-3 text-sm-right" v-if="selectedCenterProfile.phone">
                <strong>Telefonszám:</strong>
              </li>
              <li class="col-sm-9 mt-3" v-if="selectedCenterProfile.phone">
                <a :href="selectedCenterProfilePhoneClean">
                  {{ selectedCenterProfile.phone }}
                </a>
              </li>

              <li class="col-sm-3 text-sm-right" v-if="selectedCenterProfile.email">
                <strong>Email:</strong>
              </li>
              <li class="col-sm-9" v-if="selectedCenterProfile.email">
                <a :href="`mailto:${selectedCenterProfile.email}`">
                  {{ selectedCenterProfile.email }}
                </a>
              </li>

              <li class="col-sm-3 mt-3 text-sm-right">
                <strong>{{ texts.openingHours }}:</strong>
              </li>
              <li class="col-sm-9 mt-3">
                <ul v-if="selectedCenterProfile.officeHours.length" class="list-unstyled">
                  <li v-for="officeHour in selectedCenterProfile.officeHours" :key="officeHour.day" class="row">
                    <u class="col-md-12">{{ officeHour.day }}</u>
                    <div class="col-md-12">
                      <ul class="list-unstyled mb-3" v-if="officeHour.hours.length">
                        <li v-for="(hour, hourIndex) in officeHour.hours" :key="hourIndex">
                          {{ hour.timeFrom }} - {{ hour.timeTo }}
                        </li>
                      </ul>
                    </div>
                  </li>
                </ul>
                <span v-else>-</span>
              </li>
            </ul>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-success" @click="print()">
              <i class="fas fa-print"></i>
              Nyomtat
            </button>
            <button type="button" class="btn btn-secondary" data-dismiss="modal">
              {{ texts.close }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import axios from "axios";
import moment from "moment";

export default {
  name: "app",
  data() {
    return {
      accreditationStatusCodes: [],
      accreditationStatusCodesMap: {},
      additionalData: {
        days: []
      },
      counties: [],
      centerProfileStatusCodes: {
        current: {},
        empty: {},
        filled: {}
      },
      authenticated: false,
      centerProfiles: [],
      centerTypes: [],
      selectedCenterTypes: [0, 1],
      conditionBetweenSelectedCenterTypes: "|",
      selectedCenterProfile: null,
      map: null,
      markers: [],
      loading: true,
      profileSearch: null,
      profileSearchTimeout: null,
      renewalStartDate: null,
      reviewEndDate: null,
      previousRenewalEndDate: null,
      pagination: {
        currentPage: 1,
        limit: 10
      }
    };
  },
  methods: {
    initialize() {
      this.loading = true;
      axios.get(this.mapViewOptions.urls.init).then(response => {
        this.accreditationStatusCodes = response.data.accreditationStatusCodes.map(
          x => {
            return Object.assign(
              {
                selected: true,
                count: response.data.centerProfiles.filter(
                  profile => profile.basicData.accreditationStatus == x.status
                ).length
              },
              x
            );
          }
        );

        this.accreditationStatusCodesMap = {};
        this.accreditationStatusCodes.forEach(x => {
          this.accreditationStatusCodesMap[x.status] = x.text;
        });

        Object.assign(this.additionalData, response.data.additionalData);

        response.data.centerProfileStatusCodes.current.forEach(status => {
          this.centerProfileStatusCodes.current[status.status] = status.text;
        });
        response.data.centerProfileStatusCodes.empty.forEach(status => {
          this.centerProfileStatusCodes.empty[status.status] = status.text;
        });
        response.data.centerProfileStatusCodes.filled.forEach(status => {
          this.centerProfileStatusCodes.filled[status.status] = status.text;
        });

        response.data.centerTypes.forEach(centerType => {
          this.centerTypes[centerType.type] = centerType.text;
        });

        this.authenticated = response.data.authenticated;
        if (response.data.renewalStartDate) {
          this.renewalStartDate = new Date(response.data.renewalStartDate);
        }
        if (response.data.reviewEndDate) {
          this.reviewEndDate = new Date(response.data.reviewEndDate);
        }
        if (response.data.previousRenewalEndDate) {
          this.previousRenewalEndDate = new Date(
            response.data.previousRenewalEndDate
          );
        }

        this.counties = Object.keys(response.data.counties).map(key => {
          return {
            name: key,
            zipCodes: response.data.counties[key]
          };
        });

        this.centerProfiles = response.data.centerProfiles.map(profile => {
          let county = this.counties.find(
            x => x.zipCodes.indexOf(profile.basicData.centerZipCode) > -1
          );

          return Object.assign(
            {
              leader: profile.leader || {},
              centerTypeNames: profile.basicData.centerTypes.map(
                type => this.centerTypes[type]
              ),
              territorialRapporteur: null,
              contentItemId: profile.contentItemId,
              centerFullAddress:
                `${profile.basicData.centerZipCode} ${profile.basicData.centerSettlement}, ${profile.basicData.centerAddress}` +
                (county ? ` | ${county.name}` : ""),
              officeHours: profile.additional.officeHours.map(x => {
                return Object.assign(x, {
                  day: this.additionalData.days.find(d => d.key == x.day).value,
                  hours: x.hours.map(hour => {
                    return {
                      timeFrom: this.splitAndGetTimeFormatted(hour.timeFrom),
                      timeTo: this.splitAndGetTimeFormatted(hour.timeTo)
                    };
                  })
                });
              }),
              county: county ? county.name : null
            },
            profile.basicData,
            profile.renewal,
            {
              accreditationStatusDateFormat: profile.basicData
                .accreditationStatusDateUtc
                ? moment(
                    new Date(profile.basicData.accreditationStatusDateUtc)
                  ).format("YYYY.MM.DD")
                : null,
              underReview:
                profile.renewal != null &&
                profile.renewal.renewalCenterProfileStatus != null &&
                profile.renewal.renewalCenterProfileStatus != 4
            }
          );
        });

        this.centerProfiles.sort((a, b) => {
          return a.centerSettlement.localeCompare(b.centerSettlement);
        });

        this.loading = false;
        this.initMap();
        this.addSearchEventListener();
      });
    },

    clearMarkers() {
      this.markers.forEach(marker => {
        marker.setMap(null);
      });
      this.markers = [];
    },

    clearClusters() {
      if (this.markerCluster) {
        this.markerCluster.clearMarkers();
      }
    },

    initMap() {
      this.$nextTick(() => {
        let bounds = new google.maps.LatLngBounds();
        if (!this.map) {
          this.map = new google.maps.Map(
            document.getElementById("center-profiles-map"),
            {
              zoom: 13
            }
          );
        }

        let centerProfilesOnMap = this.filteredCenterProfiles.filter(
          centerProfile =>
            centerProfile.centerLatitude && centerProfile.centerLongitude
        );

        this.clearMarkers();

        let infowindow = new google.maps.InfoWindow();

        if (centerProfilesOnMap.length) {
          let locationGroups = centerProfilesOnMap.reduce((accumulator, cp) => {
            let arrayItem = accumulator.find(
              x =>
                x.centerLatitude == cp.centerLatitude &&
                x.centerLongitude == cp.centerLongitude
            );

            if (arrayItem == null) {
              arrayItem = {
                centerLatitude: cp.centerLatitude,
                centerLongitude: cp.centerLongitude,
                child: false,
                adult: false,
                centerProfiles: []
              };

              accumulator.push(arrayItem);
            }

            if (cp.centerTypes.indexOf(0) > -1) {
              arrayItem.adult = true;
            }

            if (cp.centerTypes.indexOf(1) > -1) {
              arrayItem.child = true;
            }

            arrayItem.centerProfiles.push(cp);

            return accumulator;
          }, []);

          locationGroups.forEach(group => {
            let title = "";
            let content = "";

            let centerProfilesCount = group.centerProfiles.length;

            group.centerProfiles.forEach((centerProfile, i) => {
              title += `${centerProfile.centerName} - ${centerProfile.leader.fullName}`;

              content +=
                `<div><h6>${centerProfile.centerName}</h6>` +
                `<span class="font-weight-bold">Szakellátóhely vezető:</span> ${centerProfile.leader.fullName}<br />` +
                `<span class="font-weight-bold">Cím:</span> ${centerProfile.centerZipCode} ${centerProfile.centerSettlement}, ${centerProfile.centerAddress}<br />` +
                `<span class="font-weight-bold">Profil:</span> ${centerProfile.centerTypeNames.join(
                  ", "
                )}</div>`;

              if (i < centerProfilesCount - 1) {
                title += "\n";
                content += '<hr style="border-top-width:3px" />';
              }
            });

            let icon = "";
            if (group.adult && group.child) {
              icon = this.mapMarkers.both;
            } else if (group.adult) {
              icon = this.mapMarkers.adult;
            } else if (group.child) {
              icon = this.mapMarkers.child;
            } else {
              icon = null;
            }

            let marker = new google.maps.Marker({
              position: {
                lat: group.centerLatitude,
                lng: group.centerLongitude
              },
              map: this.map,
              title: title,
              icon: icon
              //animation: google.maps.Animation.DROP
            });

            google.maps.event.addListener(
              marker,
              "click",
              (function(marker, content, infowindow) {
                return function() {
                  infowindow.setContent(content);
                  infowindow.open(this.map, marker);
                };
              })(marker, content, infowindow)
            );

            marker.setMap(this.map);
            this.markers.push(marker);
            bounds.extend(marker.position);
          });

          if (centerProfilesOnMap.length > 1) {
            this.map.fitBounds(bounds);
          } else {
            this.map.panTo({
              lat: centerProfilesOnMap[0].centerLatitude,
              lng: centerProfilesOnMap[0].centerLongitude
            });
            this.map.setZoom(15);
          }
        }
      });
    },

    loadScript(src) {
      return new Promise((resolve, reject) => {
        let script = document.createElement("script");
        script.src = src;
        script.type = "text/javascript";
        script.onload = resolve;
        script.onerror = reject;
        document.body.appendChild(script);
      });
    },

    splitAndGetTimeFormatted(timeAsString) {
      let split = timeAsString.split(":");

      return moment(new Date())
        .startOf("day")
        .add(split[0], "hours")
        .add(split[1], "minutes")
        .format("HH:mm");
    },

    loadProfile(contentItemId) {
      this.selectedCenterProfile = this.centerProfiles.find(
        x => x.contentItemId == contentItemId
      );
      if (
        !this.authenticated ||
        !this.selectedCenterProfile ||
        this.selectedCenterProfile.territorialRapporteur
      ) {
        return;
      }

      axios
        .get(this.mapViewOptions.urls.getTerritorialRapporteur, {
          params: {
            zipCode: this.selectedCenterProfile.centerZipCode,
            settlement: this.selectedCenterProfile.centerSettlement
          }
        })
        .then(response => {
          if (!response.data || response.data.length != 1) {
            return;
          }

          this.selectedCenterProfile.territorialRapporteur =
            response.data[0].territorialRapporteur;
        });
    },

    setSearch(e) {
      clearTimeout(this.profileSearchTimeout);

      this.profileSearchTimeout = setTimeout(() => {
        if (this.profileSearch != e.target.value) {
          this.profileSearch = e.target.value;
          this.initMap();
        }
      }, 500);
    },

    setSelectedCenterTypes(e, condition) {
      this.$nextTick(() => {
        this.conditionBetweenSelectedCenterTypes = condition;
        this.selectedCenterTypes = e.target.value.split(",").map(x => +x);
        this.initMap();
      });
    },

    highlightMatches(subject) {
      if (!this.searchKeywords.length) {
        return subject;
      }

      let result = (" " + subject).slice(1);
      this.searchKeywords.forEach(word => {
        let matches = [...result.matchAll(new RegExp(word, "gi"))];
        matches.forEach(m => {
          result = result.replace(
            new RegExp(`((?!\>)${m[0]}(?!\<))`, "g"),
            `<@>${m[0]}</@>`
          );
        });
      });

      return result
        .replace(/<@>/g, '<span class="highlight">')
        .replace(/<\/@>/g, "</span>");
    },

    print() {
      let stylesHtml = "";
      document
        .querySelectorAll('link[rel="stylesheet"], style')
        .forEach(node => {
          stylesHtml += node.outerHTML;
        });

      let newWindow = window.open("");
      newWindow.document.open();
      newWindow.document.write(
        `<html><head><title>${
          this.selectedCenterProfile.centerName
        }</title>${stylesHtml}</head><body>${
          document.getElementById("modal-body-center-profile").innerHTML
        }</body><html>`
      );
      newWindow.document.close();

      newWindow.onload = function() {
        newWindow.print();
      };

      newWindow.onafterprint = function() {
        newWindow.close();
      };
    },

    addSearchEventListener() {
      this.$nextTick(() => {
        document
          .getElementById("profile-search")
          .addEventListener("keyup", this.setSearch);
        // not works in ie and edge
        document
          .getElementById("profile-search")
          .addEventListener("search", this.setSearch);
      });
    },

    removeSearchEventListener() {
      document
        .getElementById("profile-search")
        .removeEventListener("keyup", this.setSearch);
      document
        .getElementById("profile-search")
        .removeEventListener("search", this.setSearch);
    }
  },
  computed: {
    searchKeywords() {
      if (!this.profileSearch) {
        return [];
      }

      return this.profileSearch
        .split(" ")
        .filter(x => !!x)
        .map(x => x.toLowerCase());
    },
    tableFields() {
      let lastRow =
        this.authenticated && (this.isRenewalPeriod || this.anyInReview);

      let result = [
        {
          key: "accreditationStatus",
          label:
            "A szakellátóhely érvényben lévő minősítési (akkreditációs) szintje",
          thStyle: `width:${lastRow ? 14 : 18}%`,
          sortable: true
        },
        {
          key: "centerName",
          label: "Szakellátóhely neve",
          sortable: true,
          thStyle: `width:${lastRow ? 22 : 24}%`
        },
        {
          key: "centerFullAddress",
          label: "Cím",
          sortable: true,
          thStyle: `width:${lastRow ? 20 : 22}%`
        },
        {
          key: "leader.fullName",
          label: "Vezető",
          sortable: true,
          thStyle: `width:${lastRow ? 12 : 15}%`
        },
        {
          key: "centerTypes",
          label: "Profil",
          sortable: true,
          thStyle: `width:${lastRow ? 17 : 22}%`
        }
      ];

      if (lastRow) {
        let headerText =
          "A szakellátóhely minősítés felülvizsgálatának jelenlegi állapota";
        if (this.isRenewalPeriod) {
          headerText += ` (Felülvizsgálat kezdete: ${moment(
            this.renewalStartDate
          ).format("YYYY.MM.DD")})`;
        }

        result.push({
          key: "renewalCenterProfileStatus",
          label: headerText,
          sortable: true,
          thStyle: "width:15px"
        });
      }

      return result;
    },
    filteredCenterProfiles() {
      let accreditationStatusFilter = this.accreditationStatusCodes
        .filter(x => x.selected)
        .map(x => x.status);

      return this.centerProfiles.filter(centerProfile => {
        if (
          accreditationStatusFilter.indexOf(
            centerProfile.accreditationStatus
          ) == -1
        ) {
          return false;
        }

        let matchByKeywords = true;
        if (this.searchKeywords && this.searchKeywords.length) {
          let fields = [
            centerProfile.centerName,
            centerProfile.leader.fullName,
            centerProfile.centerZipCode,
            centerProfile.centerSettlement,
            centerProfile.centerAddress,
            centerProfile.county
          ].filter(x => !!x);

          if (
            !this.searchKeywords.every(s => {
              return fields.some(
                prop =>
                  prop
                    .toString()
                    .toLowerCase()
                    .indexOf(s) > -1
              );
            })
          ) {
            matchByKeywords = false;
          }
        }

        if (!matchByKeywords) {
          return false;
        }

        return this.selectedCenterTypes[
          this.conditionBetweenSelectedCenterTypes == "&" ? "every" : "some"
        ](selectedType => centerProfile.centerTypes.indexOf(selectedType) > -1);
      });
    },
    texts() {
      return this.mapViewOptions.texts;
    },
    centerTypeStatistics() {
      return {
        "0,1": this.centerProfiles.length,
        "0": this.centerProfiles.filter(x => x.centerTypes.some(t => t == "0"))
          .length,
        "0,2": this.centerProfiles.filter(
          x =>
            x.centerTypes.some(t => t == "0") &&
            x.centerTypes.some(t => t == "2")
        ).length,
        "0,3": this.centerProfiles.filter(
          x =>
            x.centerTypes.some(t => t == "0") &&
            x.centerTypes.some(t => t == "3")
        ).length,
        "0,2,3": this.centerProfiles.filter(
          x =>
            x.centerTypes.some(t => t == "0") &&
            x.centerTypes.some(t => t == "2") &&
            x.centerTypes.some(t => t == "3")
        ).length,
        "1": this.centerProfiles.filter(x => x.centerTypes.some(t => t == "1"))
          .length,
        "1,3": this.centerProfiles.filter(
          x =>
            x.centerTypes.some(t => t == "1") &&
            x.centerTypes.some(t => t == "3")
        ).length
      };
    },
    isRenewalPeriod() {
      return (
        this.renewalStartDate &&
        this.reviewEndDate &&
        moment().isSameOrAfter(this.renewalStartDate) &&
        moment().isBefore(this.reviewEndDate)
      );
    },
    anyInReview() {
      return this.centerProfiles.some(x => x.underReview);
    },
    selectedCenterProfilePhoneClean() {
      if (this.selectedCenterProfile.phone) {
        return "tel:" + this.selectedCenterProfile.phone.replace(/[^+\d]/g, "");
      }
    },
    mapMarkers() {
      return this.mapViewOptions.markers;
    }
  },
  mounted() {
    this.loadScript(
      "https://maps.googleapis.com/maps/api/js?key=" +
        this.mapViewOptions.mapKey
    ).then(this.initialize, () => {
      alert("A térkép betöltése nem járt sikerrel.");
    });
  },
  beforeDestroy() {
    this.removeSearchEventListener();
  },
  props: ["mapViewOptions"]
};
</script>
