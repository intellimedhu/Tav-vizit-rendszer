<template>
  <div class="row">
    <div class="col-12">
      <div class="diary-table diary-table-mealtime" v-if="!isSmallSize">
        <!-- header -->
        <div class="row diary-table-row row-first sticky-top print-not-break">
          <div class="col diary-table-column column-first"></div>
          <div class="col diary-table-column">
            <div class="header-period first-line">
              {{ lang.down }}
            </div>
            <div class="header-period">
              {{ downPeriod[0] | shortTime }} - {{ downPeriod[1] | shortTime }}
            </div>
          </div>
          <div class="col diary-table-column">
            <div class="header-period first-line">
              {{ lang.beforeBreakfast }}
            </div>
            <div class="header-period">
              {{ breakfastPeriod[0] | shortTime }} - {{ breakfastPeriod[1] | shortTime }}
            </div>
          </div>
          <div class="col diary-table-column">
            <div class="header-period first-line">
              {{ lang.afterBreakfast }}
            </div>
            <div class="header-period">
              {{ breakfastPeriod[1] | shortTime }} - {{ breakfastPeriod[2] | shortTime }}
            </div>
          </div>
          <div class="col diary-table-column" v-if="!mealtime.elevenses">
            <div class="header-period first-line">
              {{ lang.forenoon }}
            </div>
            <div class="header-period">
              {{ forenoonPeriod[0] | shortTime }} - {{ forenoonPeriod[1] | shortTime }}
            </div>
          </div>
          <div class="col diary-table-column" v-if="mealtime.elevenses && breakfastPeriod[2] < elevensesPeriod[0]">
            <div class="header-period first-line"></div>
            <div class="header-period">
              {{ breakfastPeriod[2] | shortTime }} - {{ elevensesPeriod[0] | shortTime }}
            </div>
          </div>
          <div class="col diary-table-column" v-if="mealtime.elevenses">
            <div class="header-period first-line">
              {{ lang.beforeElevenses }}
            </div>
            <div class="header-period">
              {{ elevensesPeriod[0] | shortTime }} - {{ elevensesPeriod[1] | shortTime }}
            </div>
          </div>
          <div class="col diary-table-column" v-if="mealtime.elevenses">
            <div class="header-period first-line">
              {{ lang.afterElevenses }}
            </div>
            <div class="header-period">
              {{ elevensesPeriod[1] | shortTime }} - {{ elevensesPeriod[2] | shortTime }}
            </div>
          </div>
          <div class="col diary-table-column" v-if="mealtime.elevenses && elevensesPeriod[2] < lunchPeriod[0]">
            <div class="header-period first-line"></div>
            <div class="header-period">
              {{ elevensesPeriod[2] | shortTime }} - {{ lunchPeriod[0] | shortTime }}
            </div>
          </div>
          <div class="col diary-table-column">
            <div class="header-period first-line">
              {{ lang.beforeLunch }}
            </div>
            <div class="header-period">
              {{ lunchPeriod[0] | shortTime }} - {{ lunchPeriod[1] | shortTime }}
            </div>
          </div>
          <div class="col diary-table-column">
            <div class="header-period first-line">
              {{ lang.afterLunch }}
            </div>
            <div class="header-period">
              {{ lunchPeriod[1] | shortTime }} - {{ lunchPeriod[2] | shortTime }}
            </div>
          </div>
          <div class="col diary-table-column" v-if="!mealtime.afternoonsnack">
            <div class="header-period first-line">
              {{ lang.afternoon }}
            </div>
            <div class="header-period">
              {{ afternoonPeriod[0] | shortTime }} - {{ afternoonPeriod[1] | shortTime }}
            </div>
          </div>
          <div class="col diary-table-column" v-if="showAfternoon1">
            <div class="header-period first-line"></div>
            <div class="header-period">
              {{ lunchPeriod[2] | shortTime }} - {{ afternoonsnackPeriod[0] | shortTime }}
            </div>
          </div>
          <div class="col diary-table-column" v-if="mealtime.afternoonsnack">
            <div class="header-period first-line">
              {{ lang.beforeAfternoonsnack }}
            </div>
            <div class="header-period">
              {{ afternoonsnackPeriod[0] | shortTime }} - {{ afternoonsnackPeriod[1] | shortTime }}
            </div>
          </div>
          <div class="col diary-table-column" v-if="mealtime.afternoonsnack">
            <div class="header-period first-line">
              {{ lang.afterAfternoonsnack }}
            </div>
            <div class="header-period">
              {{ afternoonsnackPeriod[1] | shortTime }} - {{ afternoonsnackPeriod[2] | shortTime }}
            </div>
          </div>
          <div class="col diary-table-column" v-if="showAfternoon2">
            <div class="header-period first-line"></div>
            <div class="header-period">
              {{ afternoonsnackPeriod[2] | shortTime }} - {{ dinnerPeriod[0] | shortTime }}
            </div>
          </div>
          <div class="col diary-table-column">
            <div class="header-period first-line">
              {{ lang.beforeDinner }}
            </div>
            <div class="header-period">
              {{ dinnerPeriod[0] | shortTime }} - {{ dinnerPeriod[1] | shortTime }}
            </div>
          </div>
          <div class="col diary-table-column">
            <div class="header-period first-line">
              {{ lang.afterDinner }}
            </div>
            <div class="header-period">
              {{ dinnerPeriod[1] | shortTime }} - {{ dinnerPeriod[2] | shortTime }}
            </div>
          </div>
          <div class="col diary-table-column">
            <div class="header-period first-line">
              {{ lang.evening }}
            </div>
            <div class="header-period">
              {{ eveningPeriod[0] | shortTime }} - {{ eveningPeriod[1] | shortTime }}
            </div>
          </div>
        </div>

        <!-- body -->
        <div class="row diary-table-row print-not-break"
             v-for="day in daysLoaded"
             v-bind:key="day.getTime()"
             :class="{'weekend': day.getDay() == 0 || day.getDay() == 6 }">
          <div class="col diary-table-column column-first">
            <div>
              {{ day | dateFormat(dateFormatterFirstLine) }}
            </div>
            <div>
              {{ day | dateFormat(dateFormatterSecondLine) }}
            </div> 
          </div>
          <div class="col diary-table-column">
            <blood-glucose-diary-value :cell-values="periodFilter(day, downPeriod[0], downPeriod[1])"
                                       :original-unit="originalUnit"
                                       :insulin-types="insulinTypes"
                                       :bg-low-text="bgLowText"
                                       :bg-high-text="bgHighText" />
          </div>
          <div class="col diary-table-column">
            <blood-glucose-diary-value :cell-values="periodFilter(day, breakfastPeriod[0], breakfastPeriod[1])"
                                       :original-unit="originalUnit"
                                       :insulin-types="insulinTypes"
                                       :bg-low-text="bgLowText"
                                       :bg-high-text="bgHighText" />
          </div>
          <div class="col diary-table-column">
            <blood-glucose-diary-value :cell-values="periodFilter(day, breakfastPeriod[1], breakfastPeriod[2])"
                                       :original-unit="originalUnit"
                                       :insulin-types="insulinTypes"
                                       :bg-low-text="bgLowText"
                                       :bg-high-text="bgHighText" />
          </div>
          <div class="col diary-table-column" v-if="!mealtime.elevenses">
            <blood-glucose-diary-value :cell-values="periodFilter(day, forenoonPeriod[0], forenoonPeriod[1])"
                                       :original-unit="originalUnit"
                                       :insulin-types="insulinTypes"
                                       :bg-low-text="bgLowText"
                                       :bg-high-text="bgHighText" />
          </div>
          <div class="col diary-table-column" v-if="mealtime.elevenses && breakfastPeriod[2] < elevensesPeriod[0]">
            <blood-glucose-diary-value :cell-values="periodFilter(day, breakfastPeriod[2], elevensesPeriod[0])"
                                       :original-unit="originalUnit"
                                       :insulin-types="insulinTypes"
                                       :bg-low-text="bgLowText"
                                       :bg-high-text="bgHighText" />
          </div>
          <div class="col diary-table-column" v-if="mealtime.elevenses">
            <blood-glucose-diary-value :cell-values="periodFilter(day, elevensesPeriod[0], elevensesPeriod[1])"
                                       :original-unit="originalUnit"
                                       :insulin-types="insulinTypes"
                                       :bg-low-text="bgLowText"
                                       :bg-high-text="bgHighText" />
          </div>
          <div class="col diary-table-column" v-if="mealtime.elevenses">
            <blood-glucose-diary-value :cell-values="periodFilter(day, elevensesPeriod[1], elevensesPeriod[2])"
                                       :original-unit="originalUnit"
                                       :insulin-types="insulinTypes"
                                       :bg-low-text="bgLowText"
                                       :bg-high-text="bgHighText" />
          </div>
          <div class="col diary-table-column" v-if="mealtime.elevenses && elevensesPeriod[2] < lunchPeriod[0]">
            <blood-glucose-diary-value :cell-values="periodFilter(day, elevensesPeriod[2], lunchPeriod[0])"
                                       :original-unit="originalUnit"
                                       :insulin-types="insulinTypes"
                                       :bg-low-text="bgLowText"
                                       :bg-high-text="bgHighText" />
          </div>
          <div class="col diary-table-column">
            <blood-glucose-diary-value :cell-values="periodFilter(day, lunchPeriod[0], lunchPeriod[1])"
                                       :original-unit="originalUnit"
                                       :insulin-types="insulinTypes"
                                       :bg-low-text="bgLowText"
                                       :bg-high-text="bgHighText" />
          </div>
          <div class="col diary-table-column">
            <blood-glucose-diary-value :cell-values="periodFilter(day, lunchPeriod[1], lunchPeriod[2])"
                                       :original-unit="originalUnit"
                                       :insulin-types="insulinTypes"
                                       :bg-low-text="bgLowText"
                                       :bg-high-text="bgHighText" />
          </div>
          <div class="col diary-table-column" v-if="!mealtime.afternoonsnack">
            <blood-glucose-diary-value :cell-values="periodFilter(day, afternoonPeriod[0], afternoonPeriod[1])"
                                       :original-unit="originalUnit"
                                       :insulin-types="insulinTypes"
                                       :bg-low-text="bgLowText"
                                       :bg-high-text="bgHighText" />
          </div>
          <div class="col diary-table-column" v-if="showAfternoon1">
            <blood-glucose-diary-value :cell-values="periodFilter(day, lunchPeriod[2], afternoonsnackPeriod[0])"
                                       :original-unit="originalUnit"
                                       :insulin-types="insulinTypes"
                                       :bg-low-text="bgLowText"
                                       :bg-high-text="bgHighText" />
          </div>
          <div class="col diary-table-column" v-if="mealtime.afternoonsnack">
            <blood-glucose-diary-value :cell-values="periodFilter(day, afternoonsnackPeriod[0], afternoonsnackPeriod[1])"
                                       :original-unit="originalUnit"
                                       :insulin-types="insulinTypes"
                                       :bg-low-text="bgLowText"
                                       :bg-high-text="bgHighText" />
          </div>
          <div class="col diary-table-column" v-if="mealtime.afternoonsnack">
            <blood-glucose-diary-value :cell-values="periodFilter(day, afternoonsnackPeriod[1], afternoonsnackPeriod[2])"
                                       :original-unit="originalUnit"
                                       :insulin-types="insulinTypes"
                                       :bg-low-text="bgLowText"
                                       :bg-high-text="bgHighText" />
          </div>
          <div class="col diary-table-column" v-if="showAfternoon2">
            <blood-glucose-diary-value :cell-values="periodFilter(day, afternoonsnackPeriod[2], dinnerPeriod[0])"
                                       :original-unit="originalUnit"
                                       :insulin-types="insulinTypes"
                                       :bg-low-text="bgLowText"
                                       :bg-high-text="bgHighText" />
          </div>
          <div class="col diary-table-column">
            <blood-glucose-diary-value :cell-values="periodFilter(day, dinnerPeriod[0], dinnerPeriod[1])"
                                       :original-unit="originalUnit"
                                       :insulin-types="insulinTypes"
                                       :bg-low-text="bgLowText"
                                       :bg-high-text="bgHighText" />
          </div>
          <div class="col diary-table-column">
            <blood-glucose-diary-value :cell-values="periodFilter(day, dinnerPeriod[1], dinnerPeriod[2])"
                                       :original-unit="originalUnit"
                                       :insulin-types="insulinTypes"
                                       :bg-low-text="bgLowText"
                                       :bg-high-text="bgHighText" />
          </div>
          <div class="col diary-table-column">
            <blood-glucose-diary-value :cell-values="periodFilter(day, eveningPeriod[0], new Date(0, 0, 0, 23, 59, 59, 990))"
                                       :original-unit="originalUnit"
                                       :insulin-types="insulinTypes"
                                       :bg-low-text="bgLowText"
                                       :bg-high-text="bgHighText" />
          </div> 
        </div>

        <div class="row d-print-none">
          <div class="col-12 text-center pt-3">
            <button type="button"
                    class="btn btn-sm btn-outline-primary"
                    @click="loadNextDays()"
                    v-show="hasUnloadedDays">
              <i class="icon-refresh"></i>
              {{ moreButtonText }}
            </button>
          </div>
        </div>
      </div>

      <div class="row diary-sm diary-sm-mealtime" v-if="isSmallSize">
        <blood-glucose-diary-pager />

        <div class="col-12 diary-day">
            <div class="diary-rows">
              <!-- down -->
              <div class="row diary-row">
                <div class="col-5 diary-column diary-column-hours">
                  <span>
                    <div>
                      {{ lang.down }}
                    </div>
                    {{ downPeriod[0] | shortTime }} - {{ downPeriod[1] | shortTime }}
                  </span>
                </div>
                <div class="col-7 diary-column diary-column-values align-self-center">
                  <blood-glucose-diary-value :cell-values="periodFilter(diarySelectedDay, downPeriod[0], downPeriod[1])"      
                                             :original-unit="originalUnit"
                                             :insulin-types="insulinTypes"
                                             :bg-low-text="bgLowText"
                                             :bg-high-text="bgHighText" />      
                </div>
              </div>

              <!-- before breakfast -->
              <div class="row diary-row">
                <div class="col-5 diary-column diary-column-hours">
                  <span>
                    <div>
                      {{ lang.beforeBreakfast }}
                    </div>
                    {{ breakfastPeriod[0] | shortTime }} - {{ breakfastPeriod[1] | shortTime }}
                  </span>
                </div>
                <div class="col-7 diary-column diary-column-values align-self-center">
                  <blood-glucose-diary-value :cell-values="periodFilter(diarySelectedDay, breakfastPeriod[0], breakfastPeriod[1])"      
                                             :original-unit="originalUnit"
                                             :insulin-types="insulinTypes"
                                             :bg-low-text="bgLowText"
                                             :bg-high-text="bgHighText" />      
                </div>
              </div>

              <!-- after breakfast -->
              <div class="row diary-row">
                <div class="col-5 diary-column diary-column-hours">
                  <span>
                    <div>
                      {{ lang.afterBreakfast }}
                    </div>
                    {{ breakfastPeriod[1] | shortTime }} - {{ breakfastPeriod[2] | shortTime }}
                  </span>
                </div>
                <div class="col-7 diary-column diary-column-values align-self-center">
                  <blood-glucose-diary-value :cell-values="periodFilter(diarySelectedDay, breakfastPeriod[1], breakfastPeriod[2])"      
                                             :original-unit="originalUnit"
                                             :insulin-types="insulinTypes"
                                             :bg-low-text="bgLowText"
                                             :bg-high-text="bgHighText" />      
                </div>
              </div>

              <!-- forenoon -->
              <div class="row diary-row" v-if="!mealtime.elevenses">
                <div class="col-5 diary-column diary-column-hours">
                  <span>
                    <div>
                      {{ lang.forenoon }}
                    </div>
                    {{ forenoonPeriod[0] | shortTime }} - {{ forenoonPeriod[1] | shortTime }}
                  </span>
                </div>
                <div class="col-7 diary-column diary-column-values align-self-center">
                  <blood-glucose-diary-value :cell-values="periodFilter(diarySelectedDay, forenoonPeriod[0], forenoonPeriod[1])"      
                                             :original-unit="originalUnit"
                                             :insulin-types="insulinTypes"
                                             :bg-low-text="bgLowText"
                                             :bg-high-text="bgHighText" />      
                </div>
              </div>

              <!-- forenoon 1-->
              <div class="row diary-row" v-if="mealtime.elevenses && breakfastPeriod[2] < elevensesPeriod[0]">
                <div class="col-5 diary-column diary-column-hours">
                  <span>
                    <div>
                      <!-- {{ lang.forenoon }} -->
                    </div>
                    {{ breakfastPeriod[2] | shortTime }} - {{ elevensesPeriod[0] | shortTime }}
                  </span>
                </div>
                <div class="col-7 diary-column diary-column-values align-self-center">
                  <blood-glucose-diary-value :cell-values="periodFilter(diarySelectedDay, breakfastPeriod[2], elevensesPeriod[0])"      
                                             :original-unit="originalUnit"
                                             :insulin-types="insulinTypes"
                                             :bg-low-text="bgLowText"
                                             :bg-high-text="bgHighText" />      
                </div>
              </div>

              <!-- before elevenses -->
              <div class="row diary-row" v-if="mealtime.elevenses">
                <div class="col-5 diary-column diary-column-hours">
                  <span>
                    <div>
                      {{ lang.beforeElevenses }}
                    </div>
                    {{ elevensesPeriod[0] | shortTime }} - {{ elevensesPeriod[1] | shortTime }}
                  </span>
                </div>
                <div class="col-7 diary-column diary-column-values align-self-center">
                  <blood-glucose-diary-value :cell-values="periodFilter(diarySelectedDay, elevensesPeriod[0], elevensesPeriod[1])"      
                                             :original-unit="originalUnit"
                                             :insulin-types="insulinTypes"
                                             :bg-low-text="bgLowText"
                                             :bg-high-text="bgHighText" />      
                </div>
              </div>

              <!-- after elevenses -->
              <div class="row diary-row" v-if="mealtime.elevenses">
                <div class="col-5 diary-column diary-column-hours">
                  <span>
                    <div>
                      {{ lang.afterElevenses }}
                    </div>
                    {{ elevensesPeriod[1] | shortTime }} - {{ elevensesPeriod[2] | shortTime }}
                  </span>
                </div>
                <div class="col-7 diary-column diary-column-values align-self-center">
                  <blood-glucose-diary-value :cell-values="periodFilter(diarySelectedDay, elevensesPeriod[1], elevensesPeriod[2])"
                                             :original-unit="originalUnit"
                                             :insulin-types="insulinTypes"
                                             :bg-low-text="bgLowText"
                                             :bg-high-text="bgHighText" />      
                </div>
              </div>

              <!-- forenoon 2 -->
              <div class="row diary-row" v-if="mealtime.elevenses && elevensesPeriod[2] < lunchPeriod[0]">
                <div class="col-5 diary-column diary-column-hours">
                  <span>
                    <div>
                      <!-- {{ lang.forenoon }} -->
                    </div>
                    {{ elevensesPeriod[2] | shortTime }} - {{ lunchPeriod[0] | shortTime }}
                  </span>
                </div>
                <div class="col-7 diary-column diary-column-values align-self-center">
                  <blood-glucose-diary-value :cell-values="periodFilter(diarySelectedDay, elevensesPeriod[2], lunchPeriod[0])"
                                             :original-unit="originalUnit"
                                             :insulin-types="insulinTypes"
                                             :bg-low-text="bgLowText"
                                             :bg-high-text="bgHighText" />      
                </div>
              </div>

              <!-- before lunch -->
              <div class="row diary-row">
                <div class="col-5 diary-column diary-column-hours">
                  <span>
                    <div>
                      {{ lang.beforeLunch }}
                    </div>
                    {{ lunchPeriod[0] | shortTime }} - {{ lunchPeriod[1] | shortTime }}
                  </span>
                </div>
                <div class="col-7 diary-column diary-column-values align-self-center">
                  <blood-glucose-diary-value :cell-values="periodFilter(diarySelectedDay, lunchPeriod[0], lunchPeriod[1])"      
                                             :original-unit="originalUnit"
                                             :insulin-types="insulinTypes"
                                             :bg-low-text="bgLowText"
                                             :bg-high-text="bgHighText" />      
                </div>
              </div>

              <!-- after lunch -->
              <div class="row diary-row">
                <div class="col-5 diary-column diary-column-hours">
                  <span>
                    <div>
                      {{ lang.afterLunch }}
                    </div>
                    {{ lunchPeriod[1] | shortTime }} - {{ lunchPeriod[2] | shortTime }}
                  </span>
                </div>
                <div class="col-7 diary-column diary-column-values align-self-center">
                  <blood-glucose-diary-value :cell-values="periodFilter(diarySelectedDay, lunchPeriod[1], lunchPeriod[2])"      
                                             :original-unit="originalUnit"
                                             :insulin-types="insulinTypes"
                                             :bg-low-text="bgLowText"
                                             :bg-high-text="bgHighText" />      
                </div>
              </div>

              <!-- afternoon -->
              <div class="row diary-row" v-if="!mealtime.afternoonsnack">
                <div class="col-5 diary-column diary-column-hours">
                  <span>
                    <div>
                      {{ lang.afternoon }}
                    </div>
                    {{ afternoonPeriod[0] | shortTime }} - {{ afternoonPeriod[1] | shortTime }}
                  </span>
                </div>
                <div class="col-7 diary-column diary-column-values align-self-center">
                  <blood-glucose-diary-value :cell-values="periodFilter(diarySelectedDay, afternoonPeriod[0], afternoonPeriod[1])"      
                                             :original-unit="originalUnit"
                                             :insulin-types="insulinTypes"
                                             :bg-low-text="bgLowText"
                                             :bg-high-text="bgHighText" />      
                </div>
              </div>

              <!-- afternoon 1 -->
              <div class="row diary-row" v-if="showAfternoon1">
                <div class="col-5 diary-column diary-column-hours">
                  <span>
                    <div>
                      <!-- {{ lang.afternoon }} -->
                    </div>
                    {{ lunchPeriod[2] | shortTime }} - {{ afternoonsnackPeriod[0] | shortTime }}
                  </span>
                </div>
                <div class="col-7 diary-column diary-column-values align-self-center">
                  <blood-glucose-diary-value :cell-values="periodFilter(diarySelectedDay, lunchPeriod[2], afternoonsnackPeriod[0])"      
                                             :original-unit="originalUnit"
                                             :insulin-types="insulinTypes"
                                             :bg-low-text="bgLowText"
                                             :bg-high-text="bgHighText" />      
                </div>
              </div>

              <!-- before afternoonsnack -->
              <div class="row diary-row" v-if="mealtime.afternoonsnack">
                <div class="col-5 diary-column diary-column-hours">
                  <span>
                    <div>
                      {{ lang.beforeAfternoonsnack }}
                    </div>
                    {{ afternoonsnackPeriod[0] | shortTime }} - {{ afternoonsnackPeriod[1] | shortTime }}
                  </span>
                </div>
                <div class="col-7 diary-column diary-column-values align-self-center">
                  <blood-glucose-diary-value :cell-values="periodFilter(diarySelectedDay, afternoonsnackPeriod[0], afternoonsnackPeriod[1])"      
                                             :original-unit="originalUnit"
                                             :insulin-types="insulinTypes"
                                             :bg-low-text="bgLowText"
                                             :bg-high-text="bgHighText" />
                </div>
              </div>

              <!-- after afternoonsnack -->
              <div class="row diary-row" v-if="mealtime.afternoonsnack">
                <div class="col-5 diary-column diary-column-hours">
                  <span>
                    <div>
                      {{ lang.afterAfternoonsnack }}
                    </div>
                    {{ afternoonsnackPeriod[1] | shortTime }} - {{ afternoonsnackPeriod[2] | shortTime }}
                  </span>
                </div>
                <div class="col-7 diary-column diary-column-values align-self-center">
                  <blood-glucose-diary-value :cell-values="periodFilter(diarySelectedDay, afternoonsnackPeriod[1], afternoonsnackPeriod[2])"      
                                             :original-unit="originalUnit"
                                             :insulin-types="insulinTypes"
                                             :bg-low-text="bgLowText"
                                             :bg-high-text="bgHighText" />
                </div>
              </div>

              <!-- afternoon 2 -->
              <div class="row diary-row" v-if="showAfternoon2">
                <div class="col-5 diary-column diary-column-hours">
                  <span>
                    <div>
                      <!-- {{ lang.afternoon }} -->
                    </div>
                    {{ afternoonsnackPeriod[2] | shortTime }} - {{ dinnerPeriod[0] | shortTime }}
                  </span>
                </div>
                <div class="col-7 diary-column diary-column-values align-self-center">
                  <blood-glucose-diary-value :cell-values="periodFilter(diarySelectedDay, afternoonsnackPeriod[2], dinnerPeriod[0])"      
                                             :original-unit="originalUnit"
                                             :insulin-types="insulinTypes"
                                             :bg-low-text="bgLowText"
                                             :bg-high-text="bgHighText" />
                </div>
              </div>

              <!-- before dinner -->
              <div class="row diary-row">
                <div class="col-5 diary-column diary-column-hours">
                  <span>
                    <div>
                      {{ lang.beforeDinner }}
                    </div>
                    {{ dinnerPeriod[0] | shortTime }} - {{ dinnerPeriod[1] | shortTime }}
                  </span>
                </div>
                <div class="col-7 diary-column diary-column-values align-self-center">
                  <blood-glucose-diary-value :cell-values="periodFilter(diarySelectedDay, dinnerPeriod[0], dinnerPeriod[1])"      
                                             :original-unit="originalUnit"
                                             :insulin-types="insulinTypes"
                                             :bg-low-text="bgLowText"
                                             :bg-high-text="bgHighText" />      
                </div>
              </div>

              <!-- after dinner -->
              <div class="row diary-row">
                <div class="col-5 diary-column diary-column-hours">
                  <span>
                    <div>
                      {{ lang.afterDinner }}
                    </div>
                    {{ dinnerPeriod[1] | shortTime }} - {{ dinnerPeriod[2] | shortTime }}
                  </span>
                </div>
                <div class="col-7 diary-column diary-column-values align-self-center">
                  <blood-glucose-diary-value :cell-values="periodFilter(diarySelectedDay, dinnerPeriod[1], dinnerPeriod[2])"      
                                             :original-unit="originalUnit"
                                             :insulin-types="insulinTypes"
                                             :bg-low-text="bgLowText"
                                             :bg-high-text="bgHighText" />      
                </div>
              </div>

              <!-- evening -->
              <div class="row diary-row">
                <div class="col-5 diary-column diary-column-hours">
                  <span>
                    <div>
                      {{ lang.evening }}
                    </div>
                    {{ eveningPeriod[0] | shortTime }} - {{ eveningPeriod[1] | shortTime }}
                  </span>
                </div>
                <div class="col-7 diary-column diary-column-values align-self-center">
                  <blood-glucose-diary-value :cell-values="periodFilter(diarySelectedDay, eveningPeriod[0], new Date(0, 0, 0, 23, 59, 59, 999))"      
                                             :original-unit="originalUnit"
                                             :insulin-types="insulinTypes"
                                             :bg-low-text="bgLowText"
                                             :bg-high-text="bgHighText" />      
                </div>
              </div>
            </div>
        </div>
      </div>
    </div>
    
  </div>
</template>

<script>
import moment from "moment";
import bloodGlucoseDiaryPager from "./blood-glucose-diary-pager.vue";
import bloodGlucoseDiaryValue from "./blood-glucose-diary-value.vue";
import utils, { betweenDateRange } from "../services/utils";
import { EventBus } from "../services/event-bus";

export default {
  data() {
    return {
      beginDate: null,
      isSmallSize: false
    };
  },
  components: {
    bloodGlucoseDiaryPager,
    bloodGlucoseDiaryValue
  },
  filters: {
    dateFormat(date, format) {
      return moment(date).format(format);
    }
  },
  methods: {
    periodFilter(day, periodStart, periodEnd) {
      if (!this.glucoseValues || !this.insulinValues) {
        return [];
      }

      let minDate = moment(day)
        .startOf("day")
        .add(periodStart.getHours(), "hours")
        .add(periodStart.getMinutes(), "minutes")
        .toDate();

      let maxDate = moment(day)
        .startOf("day")
        .add(periodEnd.getHours(), "hours")
        .add(periodEnd.getMinutes(), "minutes")
        .toDate();

      return this.glucoseValues
        .filter(x => betweenDateRange(x.date, minDate, maxDate))
        .concat(
          this.insulinValues.filter(x =>
            betweenDateRange(x.date, minDate, maxDate)
          )
        )
        .sort(utils.sortByDate);
    },

    isSmallWindow() {
      return window.innerWidth < 1024;
    },

    getInitialBeginDate() {
      return moment(new Date(this.selectedMaxDate))
        .subtract(3, "weeks")
        .endOf("week")
        .add(1, "days")
        .toDate();
    },

    loadNextDays() {
      this.beginDate = moment(this.beginDate)
        .subtract(3, "weeks")
        .toDate();
    },

    raiseSmDateChanged() {
      EventBus.$emit("diary-sm-date-changed");
    },

    onVResized() {
      this.isSmallSize = this.isSmallWindow();
    },

    onRangeChanged() {
      this.beginDate = this.getInitialBeginDate();

      this.$store.commit(
        "setDiaryDate",
        moment(this.selectedMaxDate)
          .startOf("day")
          .toDate()
      );
      this.raiseSmDateChanged();
    }
  },
  computed: {
    globalSettings() {
      return this.$store.state.globalSettingsModule.globalSettings;
    },

    lang() {
      return this.$i18n.t(
        "bloodGlucoseDiary",
        this.globalSettings.currentLanguage
      );
    },

    selectedMinDate() {
      return this.$store.state.mainModule.range.selectedMinDate;
    },

    selectedMaxDate() {
      return this.$store.state.mainModule.range.selectedMaxDate;
    },

    diarySelectedDay() {
      return this.$store.state.mainModule.diarySelectedDay;
    },

    bgLowText() {
      return this.$i18n.t("bgLowText", this.globalSettings.currentLanguage);
    },

    bgHighText() {
      return this.$i18n.t("bgHighText", this.globalSettings.currentLanguage);
    },

    moreButtonText() {
      return this.$i18n.t(
        "moreButtonText",
        this.globalSettings.currentLanguage
      );
    },

    hasUnloadedDays() {
      return moment(this.beginDate).isAfter(this.selectedMinDate);
    },

    days() {
      let result = [];

      for (
        let currentDate = moment(this.selectedMaxDate)
          .startOf("day")
          .toDate();
        moment(currentDate).isSameOrAfter(this.selectedMinDate);
        currentDate = utils.addDays(currentDate, -1)
      ) {
        result.push(currentDate);
      }

      return result;
    },

    daysLoaded() {
      return this.days.filter(day => utils.dayLoaded(this.beginDate, day));
    },

    dateFormatterFirstLine() {
      return this.lang.dateFormatterFirstLine;
    },

    dateFormatterSecondLine() {
      return this.lang.dateFormatterSecondLine;
    },

    datepickerFormat() {
      return this.$i18n.t(
        "datepickerFormat",
        this.globalSettings.currentLanguage
      );
    },

    mealtime() {
      return this.$store.getters.patientMealtimes;
    },

    downPeriod() {
      return [
        new Date(0, 0, 0),
        utils.addMinutes(
          new Date(
            0,
            0,
            0,
            this.mealtime.breakfast.getHours(),
            this.mealtime.breakfast.getMinutes()
          ),
          -this.globalSettings.preMealTimeInMinutes
        )
      ];
    },

    breakfastPeriod() {
      let periodEnd = new Date(
        0,
        0,
        0,
        this.mealtime.breakfast.getHours(),
        this.mealtime.breakfast.getMinutes()
      );

      return [
        utils.addMinutes(periodEnd, -this.globalSettings.preMealTimeInMinutes),
        periodEnd,
        utils.addMinutes(periodEnd, this.globalSettings.postMealTimeInMinutes)
      ];
    },

    elevensesPeriod() {
      if (!this.mealtime.elevenses) {
        return;
      }

      let periodEnd = new Date(
        0,
        0,
        0,
        this.mealtime.elevenses.getHours(),
        this.mealtime.elevenses.getMinutes()
      );

      return [
        utils.addMinutes(periodEnd, -this.globalSettings.preMealTimeInMinutes),
        periodEnd,
        utils.addMinutes(periodEnd, this.globalSettings.postMealTimeInMinutes)
      ];
    },

    forenoonPeriod() {
      if (this.mealtime.elevenses) {
        return;
      }

      return [
        utils.addMinutes(
          new Date(
            0,
            0,
            0,
            this.mealtime.breakfast.getHours(),
            this.mealtime.breakfast.getMinutes()
          ),
          this.globalSettings.postMealTimeInMinutes
        ),
        utils.addMinutes(
          new Date(
            0,
            0,
            0,
            this.mealtime.lunch.getHours(),
            this.mealtime.lunch.getMinutes()
          ),
          -this.globalSettings.preMealTimeInMinutes
        )
      ];
    },

    lunchPeriod() {
      let periodEnd = new Date(
        0,
        0,
        0,
        this.mealtime.lunch.getHours(),
        this.mealtime.lunch.getMinutes()
      );

      return [
        utils.addMinutes(periodEnd, -this.globalSettings.preMealTimeInMinutes),
        periodEnd,
        utils.addMinutes(periodEnd, this.globalSettings.postMealTimeInMinutes)
      ];
    },

    afternoonPeriod() {
      if (this.mealtime.afternoonsnack) {
        return;
      }

      return [
        utils.addMinutes(
          new Date(
            0,
            0,
            0,
            this.mealtime.lunch.getHours(),
            this.mealtime.lunch.getMinutes()
          ),
          this.globalSettings.postMealTimeInMinutes
        ),
        utils.addMinutes(
          new Date(
            0,
            0,
            0,
            this.mealtime.dinner.getHours(),
            this.mealtime.dinner.getMinutes()
          ),
          -this.globalSettings.preMealTimeInMinutes
        )
      ];
    },

    afternoonsnackPeriod() {
      if (!this.mealtime.afternoonsnack) {
        return;
      }

      let periodEnd = new Date(
        0,
        0,
        0,
        this.mealtime.afternoonsnack.getHours(),
        this.mealtime.afternoonsnack.getMinutes()
      );

      return [
        utils.addMinutes(periodEnd, -this.globalSettings.preMealTimeInMinutes),
        periodEnd,
        utils.addMinutes(periodEnd, this.globalSettings.postMealTimeInMinutes)
      ];
    },

    dinnerPeriod() {
      let periodEnd = new Date(
        0,
        0,
        0,
        this.mealtime.dinner.getHours(),
        this.mealtime.dinner.getMinutes()
      );

      return [
        utils.addMinutes(periodEnd, -this.globalSettings.preMealTimeInMinutes),
        periodEnd,
        utils.addMinutes(periodEnd, this.globalSettings.postMealTimeInMinutes)
      ];
    },

    eveningPeriod() {
      return [
        utils.addMinutes(
          new Date(
            0,
            0,
            0,
            this.mealtime.dinner.getHours(),
            this.mealtime.dinner.getMinutes()
          ),
          this.globalSettings.postMealTimeInMinutes
        ),
        new Date(0, 0, 0)
      ];
    },

    showAfternoon1() {
      return (
        this.mealtime.afternoonsnack &&
        this.lunchPeriod[2] < this.afternoonsnackPeriod[0]
      );
    },

    showAfternoon2() {
      return (
        this.mealtime.afternoonsnack &&
        this.afternoonsnackPeriod[2] < this.dinnerPeriod[0]
      );
    }
  },
  mounted() {
    this.isSmallSize = this.isSmallWindow();
    this.beginDate = this.getInitialBeginDate();
    if (!this.diarySelectedDay) {
      this.$store.commit(
        "setDiaryDate",
        moment(this.selectedMaxDate)
          .startOf("day")
          .toDate()
      );
      this.raiseSmDateChanged();
    }

    EventBus.$on("vResized", this.onVResized);
    EventBus.$on("rangeChanged", this.onRangeChanged);
  },
  updated() {
    utils.triggerInvalidatedEvent("diary");
  },
  beforeDestroy() {
    EventBus.$off("vResized", this.onVResized);
    EventBus.$off("rangeChanged", this.onRangeChanged);
  },
  props: ["glucoseValues", "insulinValues", "insulinTypes", "originalUnit"]
};
</script>
